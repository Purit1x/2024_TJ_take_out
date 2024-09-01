<script setup>
import { ref, onMounted, onBeforeUnmount } from 'vue';
import { useStore } from 'vuex';
import { useRoute, useRouter } from 'vue-router';
import { getOrdersWithinThisMonth, riderInfo, getRiderPrice } from '@/api/rider';

const store = useStore();
const router = useRouter();
const rider = ref({});
const orderNum = ref(0);
const totalWageWithinThisMonth = ref(0);

let updateWageStatInterval = null;
onMounted(async () => {
    const riderData = store.state.rider;  // 登录后，对应骑手信息会被存储到本地，将本地存储的骑手信息存储至riderData中
    if (riderData) {
        rider.value = riderData;
        const res = await riderInfo(rider.value.RiderId);
        rider.value = res.data;
        console.log('骑手信息', rider.value);
    }
    else {
        router.push('/login');  // 未登录，跳转至登陆界面
    }

    await renewWageStat();
    updateWageStatInterval = setInterval(renewWageStat, 10000);  // 10秒更新一次订单数据
})
onBeforeUnmount(() => {
    if (updateWageStatInterval) {
        clearInterval(updateWageStatInterval);  // 清除定时器
    }
})

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
</template>

<style scoped></style>