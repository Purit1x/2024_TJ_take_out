<script setup>
import { useStore } from "vuex";
import { ElMessage,ElMessageBox } from "element-plus"; 
import { ref, onMounted, watch ,onBeforeUnmount} from 'vue';  
import { useRouter } from 'vue-router';
import {getEcoInfo,getFinishedOrders} from '@/api/platform';

const store = useStore();
const router = useRouter();
const finishedOrders=ref([]);
const gobackHome = () => {
    router.push('/platform-home');
}
let updateInterval=null;
const ecoInfo = ref(0)

onMounted(async() => {
     await renewOrders();
     updateInterval = setInterval(renewOrders, 10000); 
     await getEcoInfo().then(res => {  // 环保订单比例
        console.log(res)
        ecoInfo.value = res;  
       }).catch(err => {  
        ElMessage.error('获取环保比例失败'); 
    }); 
})
onBeforeUnmount(() => {  
    if (updateInterval) {  
        clearInterval(updateInterval); // 清除定时器  
    }  
});
const renewOrders=async()=>{
    try{
        const finishedOrderData= await getFinishedOrders();
        if(finishedOrderData===0){
            ElMessage.success('无已完成订单');
            finishedOrders.value=[];
        }
        else{
            finishedOrders.value=finishedOrderData.data;
        }
        console.log('orders',finishedOrders.value);
    }catch(error)
    {
        throw error;
    }
}



</script>

<template>
    <div>
        <h2>
            订单管理

            <button @click="gobackHome">返回</button>
        </h2>            
        <p> 环保订单比例： {{ecoInfo.ecoOrderRatio}}</p>
        <h2>已完成订单</h2>
        <div class="orders-scroll">
            <div
                class="order-item"
                v-for="(order,index) in finishedOrders"
                :key="index"
                :style="{ backgroundColor: hover ? 'rgba(255, 255, 204, 0.8)' : 'rgba(249, 249, 249, 1)' }"
            >
                <div>订单号：{{order.orderId}}</div>
                <div>订单总价：{{order.price}}元</div>
                <div>订单创建时间：{{ order.orderTimestamp }}</div>
                
            </div>

        </div>
       
    </div>
</template>


<style scoped>
.orders-scroll {
  max-height: 600px; /* 设置订单区域的最大高度 */
  display: flex;
  flex-direction: column;
  overflow-y: auto; /* 使订单区域可以滚动 */
  margin-left: 20px;
}
.order-item {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    padding: 10px;
    margin: 0 80px;     
    margin-bottom: 10px;
    border: 2px solid #ffee00;
    border-radius: 8px;
    background-color: #f9f9f9;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  }

.order-item div{
  margin-left: 20px;
  font-size: 16px;
  flex:1 1 50%;
  box-sizing: border-box;
}
</style>