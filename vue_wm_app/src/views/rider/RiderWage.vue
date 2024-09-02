<script setup>
import { ref, onMounted, onBeforeUnmount } from 'vue';
import { useStore } from 'vuex';
import { useRoute, useRouter } from 'vue-router';
import { getOrdersWithinThisMonth, riderInfo, getRiderPrice, getFinishedOrders } from '@/api/rider';
import dayjs from 'dayjs';

const store = useStore();
const router = useRouter();
const rider = ref({});
const orderNum = ref(0);
const totalWageWithinThisMonth = ref(0);
const finishedOrders = ref([]);

let updateWageStatInterval = null;
let updateFinishedOrdersInterval = null;
onMounted(async () => {
    const riderData = store.state.rider;  // 登录后，对应骑手信息会被存储到本地，将本地存储的骑手信息存储至riderData中
    if (riderData) {
        rider.value = riderData;
        const res = await riderInfo(rider.value.RiderId);
        rider.value = res.data;
        // console.log('骑手信息', rider.value);
    }
    else {
        router.push('/login');  // 未登录，跳转至登陆界面
    }

    await renewWageStat();
    updateWageStatInterval = setInterval(renewWageStat, 10000);  // 10秒更新一次订单数据
    await renewFinishedOrders();
    updateFinishedOrdersInterval = setInterval(renewFinishedOrders, 10000);
})
onBeforeUnmount(() => {
    if (updateWageStatInterval) {
        clearInterval(updateWageStatInterval);  // 清除定时器
    }
    if (updateFinishedOrdersInterval) {
        clearInterval(updateFinishedOrdersInterval);
    }
})
const renewFinishedOrders = async () => {
    try {
        const finishedOrdersData = await getFinishedOrders(rider.value.riderId);
        // console.log("后端原始数据为：", finishedOrdersData);
        if (finishedOrdersData !== 0) {  // 已送达订单列表非空
            const promise = await finishedOrdersData.map(async orderItem => {
                return {
                    orderId: orderItem.orderId,
                    deliveryFee: await getRiderPrice(orderItem.orderId) || "加载中",
                    dateTime: dayjs(orderItem.orderTimestamp).format('YYYY-MM-DD (+8)HH:mm')
                };
            })
            finishedOrders.value = await Promise.all(promise);  // 等待所有映射完成
            finishedOrders.value.sort((a, b) => b.orderId - a.orderId);
            // console.log("处理后的数据为", finishedOrders.value);
        }
    }
    catch (error) {
        throw error;
    }
}
const renewWageStat = async () => {
    try {
        const ordersData = await getOrdersWithinThisMonth(rider.value.riderId);
        if (ordersData === 0) {  // 列表为空, 后端返回0
            orderNum.value = 0;
            totalWageWithinThisMonth.value = 0;
            return;
        }
        else {
            orderNum.value = ordersData.length;
            // 将列表中的每一项OrderId映射为配送费
            totalWageWithinThisMonth.value = 0;
            const promise = await ordersData.map(orderItem => getRiderPrice(orderItem));

            const fees = await Promise.all(promise);  // 等待所有异步获取完成
            for (const fee of fees) {
                totalWageWithinThisMonth.value += fee;  // 累加配送费
            }
        }
    }
    catch (error) {
        throw error;
    }
}

function displayTotalWageWithinThisMonth() {
    return totalWageWithinThisMonth.value || '加载中...';
}
</script>

<template>
    <div>
        <br><br>
        <h2>您所在的位置：工资记录</h2>
        <h3>您本月送单总量：{{ orderNum }} 您本月获得的总配送费为： {{ displayTotalWageWithinThisMonth() }}</h3>
    </div>
    <div class="fees-scroll">
        <el-scrollbar max-height="500px">
            <el-table :data="finishedOrders" border stripe>
                <el-table-column prop="orderId" label="订单号" width="200" />
                <el-table-column prop="deliveryFee" label="配送费" width="200" />
                <el-table-column prop="dateTime" label="订单时间" />
            </el-table>
        </el-scrollbar>
    </div>
</template>

<style scoped>
.fees-scroll {
    max-height: 600px;
    /* 设置订单区域的最大高度 */
    display: flex;
    flex-direction: column;
    justify-content: center;
    border: 1px solid transparent;
    overflow-y: auto;
    /* 使订单区域可以滚动 */
    margin-left: 20px;
    width: 70%;
}

.fee-item {
    padding: 10px 0;
    border: 1px solid #ccc;
    margin: 10px;
    border-radius: 4px;
    background-color: white;
}
</style>