<script setup>
import { useStore } from "vuex";
import { ElMessage,ElMessageBox } from "element-plus"; 
import { ref, onMounted, watch ,onBeforeUnmount} from 'vue';  
import { useRouter } from 'vue-router';
import {getEcoInfo,getFinishedOrders,getUnfinishedOrders,deleteFinishedOrder} from '@/api/platform';

const store = useStore();
const router = useRouter();
const finishedOrders=ref([]);
const unFinishedOrders=ref([]);
const gobackHome = () => {
    router.push('/platform-home');
}
let updateInterval=null;
const ecoInfo = ref(0)
const showState =ref(1);  //查看订单状态

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
        const unFinishedOrderData= await getUnfinishedOrders();
        if(unFinishedOrderData===0){
            ElMessage.success('无未完成订单');
            unFinishedOrders.value=[];
        }
        else{
            unFinishedOrders.value=unFinishedOrderData.data;
        }
        console.log('orders',finishedOrders.value);
    }catch(error)
    {
        throw error;
    }
}



</script>

<template>
    <div class="box">
        <div class ="head">订单管理</div>           
        <div class="subhead">
            <label 
                @click="showState=1" 
                :class="{ active: showState === 1 }"
            >已完成订单
            </label>&nbsp;&nbsp;
            <label 
                @click="showState=2" 
                :class="{ active: showState === 2 }"
            >未完成订单
            </label>
        </div>
        <div class ="orders-container">
            <div class="orders-scroll" v-if="showState === 1">
                <div
                    class="order-item"
                    v-for="(order,index) in finishedOrders"
                    :key="index"
                    :style="{ backgroundColor: hover ? 'rgba(255, 255, 204, 0.8)' : 'rgba(249, 249, 249, 1)' }"
                >
                    
                    <div>订单号：{{order.orderId}}</div>
                    <div>订单总价：{{order.price}}元</div>
                    <div>订单创建时间：{{ order.orderTimestamp }}</div>
                    <div class = "right-group"><button class="delete-btn" @click="deleteFinishedOrder(order.orderId)">删除订单</button></div>
                    
                </div>
            </div>
            <div class="orders-scroll" v-if="showState === 2">
                <div
                    class="order-item"
                    v-for="(order,index) in unFinishedOrders"
                    :key="index"
                    :style="{ backgroundColor: hover ? 'rgba(255, 255, 204, 0.8)' : 'rgba(249, 249, 249, 1)' }"
                >
                    <div>订单号：{{order.orderId}}</div>
                    <div>订单总价：{{order.price}}元</div>
                    <div>订单创建时间：{{ order.orderTimestamp }}</div>
                    
                </div>
            </div>
        </div>
        <div class="bottom">
            <span style="font-size:15px"> 环保订单比例： {{ecoInfo.ecoOrderRatio}}</span>
            <button @click="gobackHome" class = "return">返回</button>
        </div>
    </div>
</template>


<style scoped lang="scss">

.box{
    padding: 20px;
    background-color: #7ac2ee;
    border: 2px solid #000000;
    border-radius: 20px;
    margin-right: 30px;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);

    font-size: 3vmin; /* 字体大小 */
    position: fixed; /* 固定定位 */
    top: 15px; /* 贴近顶部 */
    left: 50%; /* 水平居中 */
    transform: translateX(-50%); /* 修正水平居中 */
    width: 70%;
    height: 95%;

}

.head{
    display:flex;
    justify-content: center; /* 水平居中 */
    align-items: center; /* 垂直居中 */
    left:50%;
    font-size: 4vmin; /* 字体大小 */
    color:#000000;
    margin-bottom: 1%;
}

.subhead {
  display: flex;
  align-items: center;
  justify-content: space-between; /* 均匀分布标签 */
  font-size: 20px;
  margin: 20px;
  padding: 0px;
  padding-left: 200px;
  padding-right:200px;
  background-color: #FFC0CB;
  border-radius: 40px;
  border: 2px solid #000000;
}

.bottom{
    display: flex;
    justify-content: space-between; /* 均匀分布标签 */
    padding-left:25px;
    padding-right: 25px;
}

label {
  cursor: pointer;
  color: #000000; /* 默认颜色 */
  background-color: #ffc0cb; 
  
  padding:5px;
  border-radius:40px;
  font-size:15px;
  font-weight: bold;
}

.normal-button {
  background-color: #ffcc00;
  color:black;
}

label.active {
  background-color: rgb(255, 255, 255); /* 选中时的颜色 */
  
  padding:5px;
  border-radius:40px;
}

.return{
    
    right: 10px;
    bottom: 10px;
    background-color: #FFC0CB;
    font-size: 2.5vmin;
}

.orders-container{
    background-color: #ffffff;
    border:2px solid black;
    border-radius: 20px;
    padding:5px;
    
    max-height: 410px; /* 设置订单区域的最大高度 */
    min-height: 410px;
    display: flex;
    flex-direction: column;
    overflow-y: auto; /* 使订单区域可以滚动 */
    
    margin-left: 20px;
    margin-right: 20px;
    margin-bottom:10px;
}

.orders-scroll {
  max-height: 400px; /* 设置订单区域的最大高度 */
  display: flex;
  flex-direction: column;
  overflow-y: auto; /* 使订单区域可以滚动 */
  margin-left: 10px;
  margin-top:13px;
}

/* 隐藏滚动条 */
.orders-scroll::-webkit-scrollbar {
    width: 12px;
}

/* 滚动条轨道 */
.orders-scroll::-webkit-scrollbar-track {
    background: #ffffff;
}
/* 滚动条滑块 */
.orders-scroll::-webkit-scrollbar-thumb {
    background-color: #ffffff;
    border-radius: 10px;
    border: 2px solid #000000;
}

.order-item {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    padding: 5px;
    margin-left:25px;
    margin-right:35px;   
    margin-bottom: 10px;
    border: 2px solid #ffee00;
    border-radius: 8px;
    background-color: #f9f9f9;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);

    div{
        margin-left: 20px;
        font-size: 16px;
        flex:1 1 50%;
        box-sizing: border-box;
    }

    .right-group {
      margin-right: 10px; /* 将右侧按钮推到右边 */
      display: flex;
    }

    button {
      margin-right: 5px;
      padding: 6px 12px;
      font-size: 14px;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      transition: background-color 0.3s;

        &.delete-btn {
            background-color: #f44336;
            color: white;

            &:hover {
            background-color: #ff5b58;
            }
        }
    }
  }

.return{
    padding: 10px 15px;
    position: absolute;
    right: 10px;
    bottom: 10px;
    background-color: #FFC0CB;
}
.return:hover{
    background-color: #f7ced5;
}
</style>