<script setup>
import { ref, onMounted, watch, onBeforeUnmount, computed } from 'vue';
import { ElMessage, ElMessageBox } from "element-plus";
import { useRouter } from 'vue-router';
import { useStore } from "vuex";
import { getMerchantIds, getMerchantsInfo, GetAddressByAddressId, getOrderDishes } from '@/api/user';
import { getPaidOrders, getReceivedOrders, receiveOrder, riderInfo, getRiderPrice } from '@/api/rider';
import { getMerAddrByOrderId } from '@/api/merchant';

const store = useStore();
const router = useRouter();
const showState = ref(1);  // 决定显示可接订单还是已接订单
const receivedOrders = ref([]);  // 已接订单列表
const receivableOrders = ref([]);  // 可接订单列表
const rider = ref({});  // 初始化骑手对象信息
const deliveryFees = ref({});  // 存储配送费
const merchantAddresses = ref({});  // 存储各订单商户地址
const targetAddresses = ref({});  // 存储各订单交付地址
const targetName = ref({});  // 存储交付人姓名
const targetPhone = ref({})  // 存储交付人电话
let updateInterval = null;  // 订单信息更新的计时器
let deliveryUpdateInterval = null;  // 配送费更新计时器
let merchantAddrUpdateInterval = null;  // 商户地址更新计时器
let targetAddrUpdateInterval = null;

onMounted(async () => {
    showState.value = 1;
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

    await renewRiderOrders();
    updateInterval = setInterval(renewRiderOrders, 10000);  // 每十秒更新一次订单信息
    await renewDeliveryFees();
    deliveryUpdateInterval = setInterval(renewDeliveryFees, 10000);
    await renewMerchantAddresses();
    merchantAddrUpdateInterval = setInterval(renewMerchantAddresses, 10000);
    await renewTargetAddresses();
    targetAddrUpdateInterval = setInterval(renewTargetAddresses, 10000);

})
onBeforeUnmount(() => {
    if (updateInterval) {
        clearInterval(updateInterval); // 清除定时器  
    }
});

const renewRiderOrders = async () => {
    try {
        console.log('显示状态：', showState.value);
        const receivedOrdersData = await getReceivedOrders(rider.value.riderId);
        if (receivedOrdersData.data === 20000) {
            if (showState.value === 2) {  // 仅当在已接订单页面才弹出消息
                ElMessage.success('您还未接单');
            }
            receivedOrders.value = [];
        }
        else {
            receivedOrders.value = receivedOrdersData.data;
        }
        const receivableOrdersData = await getPaidOrders(rider.value.riderId);
        if (receivableOrdersData.data === 40000) {
            if (showState.value === 1) {  // 仅当在可接订单页面才弹出消息
                ElMessage.success('当前无可接订单');
            }
            receivableOrders.value = [];
        }
        else {
            receivableOrders.value = receivableOrdersData.data;
        }
        console.log('可接订单', receivableOrders.value);
        console.log('已接订单', receivedOrders.value);
    } catch (error) {
        throw error;
    }
}
const renewDeliveryFees = async () => {  // 保存每个订单的配送费
    if (receivableOrders.value.length > 0) {
        // 创建一个Promise数组，同时加载所有订单的配送费
        const promise1 = receivableOrders.value.map(orderItem => getRiderPrice(orderItem.order.orderId));

        // 等待所有异步请求完成
        const fees1 = await Promise.all(promise1);

        for (let i = 0; i < fees1.length; i++) {
            deliveryFees.value[receivableOrders.value[i].order.orderId] = fees1[i];
        }
    }

    if (receivedOrders.value.length > 0) {
        // 创建一个Promise数组，同时加载所有订单的配送费
        const promise2 = receivedOrders.value.map(orderItem => getRiderPrice(orderItem.orderId));

        // 等待所有异步请求完成
        const fees2 = await Promise.all(promise2);

        for (let i = 0; i < fees2.length; i++) {
            deliveryFees.value[receivedOrders.value[i].orderId] = fees2[i];
        }
    }
}
const renewMerchantAddresses = async () => {  // 保存每个订单的商户地址
    if (receivableOrders.value.length > 0) {
        const promise1 = receivableOrders.value.map(orderItem => getMerAddrByOrderId(orderItem.order.orderId));
        const addrs1 = await Promise.all(promise1);
        for (let i = 0; i < addrs1.length; i++) {
            merchantAddresses.value[receivableOrders.value[i].order.orderId] = addrs1[i].data;
        }
    }

    if (receivedOrders.value.length > 0) {
        const promise2 = receivedOrders.value.map(orderItem => getMerAddrByOrderId(orderItem.orderId));
        const addrs2 = await Promise.all(promise2);
        for (let i = 0; i < addrs2.length; i++) {
            merchantAddresses.value[receivedOrders.value[i].orderId] = addrs2[i].data;
        }
        console.log('各商户地址', merchantAddresses.value);
    }
}
const renewTargetAddresses = async () => {  // 保存每个订单的交付地址
    if (receivableOrders.value.length > 0) {
        const promise1 = receivableOrders.value.map(orderItem => GetAddressByAddressId(orderItem.order.addressId));
        const addrs1 = await Promise.all(promise1);
        for (let i = 0; i < addrs1.length; i++) {
            targetAddresses.value[receivableOrders.value[i].order.orderId]
                = addrs1[i].data.userAddress + ' ' + addrs1[i].data.houseNumber + '号';
            targetName.value[receivableOrders.value[i].order.orderId] = addrs1[i].data.contactName;
            targetPhone.value[receivableOrders.value[i].order.orderId] = addrs1[i].data.phoneNumber;
        }
    }

    if (receivedOrders.value.length > 0) {
        const promise2 = receivedOrders.value.map(orderItem => GetAddressByAddressId(orderItem.addressId));
        const addrs2 = await Promise.all(promise2);
        for (let i = 0; i < addrs2.length; i++) {
            targetAddresses.value[receivedOrders.value[i].orderId]
                = addrs2[i].data.userAddress + ' ' + addrs2[i].data.houseNumber + '号';;
            targetName.value[receivedOrders.value[i].orderId] = addrs2[i].data.contactName;
            targetPhone.value[receivedOrders.value[i].orderId] = addrs2[i].data.phoneNumber;
        }
    }
}
function displayTargetAddr(orderId) {
    return targetAddresses.value[orderId] || '加载中...';
}
function displayMerchantAddr(orderId) {
    return merchantAddresses.value[orderId] || '加载中...';
}
function displayDeliveryFee(orderId) {
    return deliveryFees.value[orderId] || '加载中...';
}
function displayTargetName(orderId) {
    return targetName.value[orderId] || '加载中...';
}
function displayTargetPhone(orderId) {
    return targetPhone.value[orderId] || '加载中...';
}
async function handleReceiveOrder(data) {
    try {
        const response = await receiveOrder(data);
        console.log('Success:', response);
        // 这里可以处理成功的回调逻辑，例如显示成功提示等
    } catch (error) {
        console.error('Error:', error);
        // 这里可以处理错误的回调逻辑，例如显示错误提示等
    }
}
</script>

<template>
    <div>
        <br><br>
        <h2>您所在的位置：订单管理</h2>
        <div class="orders">
            <button @click="showState = 1">可接订单</button>&nbsp;&nbsp;
            <button @click="showState = 2">已接订单</button>
            <br><br>
            <!--显示可接订单-->
            <div class="orders-scroll" v-if="showState === 1">
                <div class="order-item" v-for="(orderItem, index) in receivableOrders" :key="index">
                    <div>订单号：{{ orderItem.order.orderId }}</div>
                    <div>配送费：{{ displayDeliveryFee(orderItem.order.orderId) }}&nbsp;元</div>
                    <div>商户地址：{{ displayMerchantAddr(orderItem.order.orderId) }}</div>
                    <div>交付地址：{{ displayTargetAddr(orderItem.order.orderId) }}</div>
                    <div>收货人：{{ displayTargetName(orderItem.order.orderId) }}</div>
                    <div>客户电话：{{ displayTargetPhone(orderItem.order.orderId) }}</div>
                    <div><button
                            @click="handleReceiveOrder({ OrderId: orderItem.order.orderId, RiderId: rider.riderId })">接单</button>
                    </div>
                </div>
            </div>
            <!--显示已接订单-->
            <div class="orders-scroll" v-if="showState === 2">
                <div class="order-item" v-for="(orderItem, index) in receivedOrders" :key="index">
                    <div>订单号：{{ orderItem.orderId }}</div>
                    <div>配送费：{{ displayDeliveryFee(orderItem.orderId) }}&nbsp;元</div>
                    <div>商户地址：{{ displayMerchantAddr(orderItem.orderId) }}</div>
                    <div>交付地址：{{ displayTargetAddr(orderItem.orderId) }}</div>
                    <div>收货人：{{ displayTargetName(orderItem.orderId) }}</div>
                    <div>客户电话：{{ displayTargetPhone(orderItem.orderId) }}</div>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
.orders-scroll {
    max-height: 600px;
    /* 设置订单区域的最大高度 */
    display: flex;
    flex-direction: column;
    border: 1px solid #ccc;
    overflow-y: auto;
    /* 使订单区域可以滚动 */
    margin-left: 20px;
}

.order-item {
    padding: 10px 0;
    border: 1px solid #ccc;
    display: flex;
    flex-wrap: wrap;
}

.order-item div {
    margin-left: 20px;
    font-size: 16px;
    flex: 1 1 50%;
    box-sizing: border-box;
}
</style>

