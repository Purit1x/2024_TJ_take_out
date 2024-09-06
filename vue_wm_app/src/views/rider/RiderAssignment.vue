<script setup>
import { ref, onMounted, watch, onBeforeUnmount } from 'vue';
import { ElMessage, ElMessageBox } from "element-plus";
import { useRouter } from 'vue-router';
import { useStore } from "vuex";
import { getMerchantIds, getMerchantsInfo, GetAddressByAddressId, getOrderDishes } from '@/api/user';
import { getPaidOrders, getReceivedOrders, receiveOrder, riderInfo, getRiderPrice,getFinishedOrders } from '@/api/rider';
import { getMerAddrByOrderId, deliverOrder } from '@/api/merchant';

const store = useStore();
const router = useRouter();
const showState = ref(1);  // 决定显示可接订单还是已接订单,已完成订单
const receivedOrders = ref([]);  // 已接订单列表
const receivableOrders = ref([]);  // 可接订单列表
const finishedOrders=ref([]);// 已完成订单列表
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

const activeName = ref('first')
const handleClick = (tab, event) => {
    if (tab.index == 0)
        showState.value = 1;
    else if (tab.index == 1)
        showState.value = 2;
    else if (tab.index == 2)
        showState.value = 3;
}

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
    if (deliveryUpdateInterval)
        clearInterval(deliveryUpdateInterval);
    if (merchantAddrUpdateInterval)
        clearInterval(merchantAddrUpdateInterval);
    if (targetAddrUpdateInterval)
        clearInterval(targetAddrUpdateInterval);
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
        const finishedOrderData=await getFinishedOrders(rider.value.riderId);
        if(finishedOrderData===0){
            if(showState.value===3){
                ElMessage.success('无已完成订单');
            }
            finishedOrders.value=[];
        }
        else{
            finishedOrders.value=finishedOrderData;
        }
        console.log('可接订单', receivableOrders.value);
        console.log('已接订单', receivedOrders.value);
        console.log('已完成订单',finishedOrders.value);
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
    if(finishedOrders.value.length>0){
        const promise3=finishedOrders.value.map(orderItem=>getRiderPrice(orderItem.orderId));
        const fees3=await Promise.all(promise3);
        for(let i=0;i<fees3.length;i++){
            deliveryFees.value[finishedOrders.value[i].orderId] = fees3[i];
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
    if (finishedOrders.value.length > 0) {
        const promise3 = finishedOrders.value.map(orderItem => getMerAddrByOrderId(orderItem.orderId));
        const addrs3 = await Promise.all(promise3);
        for (let i = 0; i < addrs3.length; i++) {
            merchantAddresses.value[finishedOrders.value[i].orderId] = addrs3[i].data;
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


    if (finishedOrders.value.length > 0) {
        const promise2 = finishedOrders.value.map(orderItem => GetAddressByAddressId(orderItem.addressId));
        const addrs2 = await Promise.all(promise2);
        for (let i = 0; i < addrs2.length; i++) {
            targetAddresses.value[finishedOrders.value[i].orderId]
                = addrs2[i].data.userAddress + ' ' + addrs2[i].data.houseNumber + '号';;
            targetName.value[finishedOrders.value[i].orderId] = addrs2[i].data.contactName;
            targetPhone.value[finishedOrders.value[i].orderId] = addrs2[i].data.phoneNumber;
        }
    }
}
function displayTargetAddr(orderId) {
    return targetAddresses.value[orderId] ||'加载中...';
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
        renewRiderOrders();
    } catch (error) {
        console.error('Error:', error);
        // 这里可以处理错误的回调逻辑，例如显示错误提示等
    }
}
async function handleDeliverOrder(data) {
    try {
        const response = await deliverOrder(data);
        console.log('Success: ', response);
        renewRiderOrders();
    } catch (error) {
        console.error('Error:', error);
    }
}
</script>

<template>
    <div>
        <h2 class="maintitle">订单管理</h2>
        <div class="orders">
            <el-tabs v-model="activeName" class="demo-tabs" @tab-click="handleClick">
                <el-tab-pane label="可接订单" name="first"></el-tab-pane>
                <el-tab-pane label="已接订单" name="second"></el-tab-pane>
                <el-tab-pane label="已完成订单" name="third"></el-tab-pane>
            </el-tabs>


            <!--显示可接订单-->
            <div class="orders-scroll" v-if="showState === 1">
                <el-scrollbar max-height="500px">
                    <div class="order-item" v-for="(orderItem, index) in receivableOrders" :key="index">
                        <el-descriptions title="订单">
                            <el-descriptions-item width="20%" label="订单号：">{{ orderItem.order.orderId }}</el-descriptions-item>
                            <el-descriptions-item width="40%" label="商户地址：">{{ displayMerchantAddr(orderItem.order.orderId)
                                }}</el-descriptions-item>
                            <el-descriptions-item width="40%" label="交付地址：">{{ displayTargetAddr(orderItem.order.orderId)
                                }}</el-descriptions-item>
                            <el-descriptions-item width="20%" label="收货人：">{{ displayTargetName(orderItem.order.orderId)
                                }}</el-descriptions-item>
                            <el-descriptions-item width="40%" label="客户电话：">{{ displayTargetPhone(orderItem.order.orderId)
                                }}</el-descriptions-item>
                            <el-descriptions-item width="40%" label="配送费：">{{ displayDeliveryFee(orderItem.order.orderId)
                                }}&nbsp;元</el-descriptions-item>
                        </el-descriptions>
                        <div>
                            <el-button type="primary"
                                @click="handleReceiveOrder({ OrderId: orderItem.order.orderId, RiderId: rider.riderId })">
                                接单<el-icon class="el-icon--right">
                                    <Check />
                                </el-icon>
                            </el-button>
                        </div>
                    </div>
                </el-scrollbar>
            </div>


            <!--显示已接订单-->
            <div class="orders-scroll" v-if="showState === 2">
                <el-scrollbar max-height="500px">
                    <div class="order-item" v-for="(orderItem, index) in receivedOrders" :key="index">
                        <el-descriptions title="订单">
                            <el-descriptions-item width="20%" label="订单号：">{{ orderItem.orderId }}</el-descriptions-item>
                            <el-descriptions-item width="40%" label="商户地址：">{{ displayMerchantAddr(orderItem.orderId)
                                }}</el-descriptions-item>
                            <el-descriptions-item width="40%" label="交付地址：">{{ displayTargetAddr(orderItem.orderId)
                                }}</el-descriptions-item>
                            <el-descriptions-item width="20%" label="收货人：">{{ displayTargetName(orderItem.orderId)
                                }}</el-descriptions-item>
                            <el-descriptions-item width="40%" label="客户电话：">{{ displayTargetPhone(orderItem.orderId)
                                }}</el-descriptions-item>
                            <el-descriptions-item width="40%" label="配送费：">{{ displayDeliveryFee(orderItem.orderId)
                                }}&nbsp;元</el-descriptions-item>
                        </el-descriptions>
                        <div>
                            <el-button type="primary" @click="handleDeliverOrder({ OrderId: orderItem.orderId })">
                                送达<el-icon class="el-icon--right">
                                    <Check />
                                </el-icon>
                            </el-button>
                        </div>
                    </div>
                </el-scrollbar>
            </div>


            <!--显示已完成订单-->
            <div class="orders-scroll" v-if="showState === 3">
                <el-scrollbar max-height="500px">
                    <div class="order-item" v-for="(orderItem, index) in finishedOrders" :key="index">
                        <el-descriptions title="订单" >
                            <el-descriptions-item width="20%" label="订单号：">{{ orderItem.orderId }}</el-descriptions-item>
                            <el-descriptions-item width="40%" label="商户地址：">{{ displayMerchantAddr(orderItem.orderId) }}</el-descriptions-item>
                            <el-descriptions-item width="40%" label="交付地址：">{{ displayTargetAddr(orderItem.orderId) }}</el-descriptions-item>
                            <el-descriptions-item width="20%" label="收货人：">{{ displayTargetName(orderItem.orderId) }}</el-descriptions-item>
                            <el-descriptions-item width="40%" label="客户电话：">{{ displayTargetPhone(orderItem.orderId) }}</el-descriptions-item>
                            <el-descriptions-item width="40%" label="配送费：">{{ displayDeliveryFee(orderItem.orderId) }}&nbsp;元</el-descriptions-item> 
                        </el-descriptions>       
                             
                    </div>
                </el-scrollbar>
            </div>
        </div>
    </div>
</template>

<style scoped>
.maintitle {
    text-align: center;
}

.demo-tabs {
    margin-left: 40px;
}

.orders-scroll {
    max-height: 600px;
    /* 设置订单区域的最大高度 */
    display: flex;
    flex-direction: column;
    border: 1px solid #ccc;
    overflow-y: auto;
    /* 使订单区域可以滚动 */
    margin-left: 20px;
    margin-right: 20px;
}

.order-item {
    padding: 10px 0;
    border: 1px solid #ccc;
    margin: 10px;
    border-radius: 4px;
    background-color: white;
}

.order-item div {
    margin-left: 20px;
    font-size: 16px;
    flex: 1 1 50%;
    box-sizing: border-box;
}
</style>
