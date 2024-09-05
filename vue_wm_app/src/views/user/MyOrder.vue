<script setup>
import { useStore } from "vuex";
import { ElMessage } from "element-plus";
import { ref, onMounted, watch,onBeforeUnmount } from 'vue';  
import { useRouter } from 'vue-router';
import { getOrders,getOrderCoupon,getOrderDishes,GetAddressByAddressId,getMerchantsInfo,GetCouponInfo,userInfo,PurchaseOrder,deleteOrder, updateOrderComment } from '@/api/user'
import { getDishInfo } from '@/api/merchant'
const refForm = ref(null);
const store = useStore();
const router = useRouter();
const userId = ref(store.state.user.userId);
const orders = ref([]);
const showState =ref(0);  //查看订单状态
const nopayedOrders = ref([]);  //未支付订单
const pendingOrders = ref([]);  //待处理订单
const deliveringOrders = ref([]);  //派送中订单
const completedOrders = ref([]);  //已完成订单
const currentOrder = ref({});  //当前订单
const isOrderInfo = ref(false);   //订单详情是否显示
const currentMerchant = ref({});  //当前商家
const showPayDialog = ref(false);  //支付弹窗
const paymentPassword = ref('');  //支付密码
const paymentError = ref('');  //支付密码错误提示
const correctPassword = ref('');  //正确的支付密码
const newOrderComment = ref({
    merchantRating: [
        {required:true,message:'请输入给商家的评分(0-5)',trigger:'blur'},
        {
            pattern:/^[0-5]{1}$/,
            message:'必须为一位数字',
            trigger:'blur',
        },
    ],
    riderRating: [
        {required:true,message:'请输入给骑手的评分(0-5)',trigger:'blur'},
        {
            pattern:/^[0-5]{1}$/,
            message:'必须为一位数字',
            trigger:'blur',
        },
    ],
    comment: [{ required: false, message: '请输入评价', trigger: 'blur' }], 
});
const submitComment=async(orderId)=>{//保存评价
    const data = {
        Id:orderId,
        MerchantRating: currentOrder.value.merchantRating,
        RiderRating: currentOrder.value.riderRating,
        Comment:currentOrder.value.comment,
    }
    console.log('评价为：',data);
    try{
        const response =await updateOrderComment(data);
        // console.log('提交评价后端返回为：',response.msg);
        if(response.msg==='订单评价更新成功'){
            ElMessage.success('更新成功');
            
        }else {
            ElMessage.error('尚未更改信息1');
        }
    }catch (error) {  
        ElMessage.error('尚未更改信息2');
    }  
}
const gobackHome = () => {
    router.push('/user-home/personal');

}

// 定时器ID  
let updateInterval = null;
onMounted(async() => {
    const userInformation=await userInfo(userId.value); 
    correctPassword.value=userInformation.data.walletPassword;  //正确的支付密码
    await renewOrders();
    updateInterval = setInterval(renewOrders, 10000); // 每10秒更新订单
});
onBeforeUnmount(() => {  
    if (updateInterval) {  
        clearInterval(updateInterval); // 清除定时器  
    }  
});  
const renewOrders = async() => {  //更新order信息
    try{
        const ordersData = await getOrders(userId.value);
        if(ordersData.data===40000)
        {
            ElMessage.success('无订单');
            orders.value = [];
            nopayedOrders.value = [];
            pendingOrders.value = [];
            deliveringOrders.value = [];
            completedOrders.value = [];
            return;
        }
        orders.value = ordersData.data;
        
        for(let i=0;i<orders.value.length;i++){
            const addressData= await GetAddressByAddressId(orders.value[i].addressId);
            orders.value[i].address=addressData.data;
            const orderDishesData = await getOrderDishes(orders.value[i].orderId);
            orders.value[i].dishes=orderDishesData.data;
            const orderCouponData = await getOrderCoupon(orders.value[i].orderId);
            orders.value[i].coupon=orderCouponData.data;
        }
        //nopayedOrders.value = orders.value.filter(order => order.state === 0);
         // 处理未支付订单，添加倒计时  
         nopayedOrders.value = orders.value  
            .filter(order => order.state === 0)  
            .map(order => {  
                // 计算离过期时间  
                const orderCreationTime = new Date(order.orderTimestamp).getTime();
                const currentTime = new Date().getTime();  
                const timeDiff = (currentTime - orderCreationTime-8*60*60*1000);  
                // 如果超过15分钟，返回null，后面会通过filter删除  
                if (timeDiff > 15 * 60 * 1000){
                    return null;}
                // 添加倒计时属性  
                return {   
                    ...order,   
                    countdown: Math.max(0, (15 * 60 * 1000 - timeDiff) / 1000)  // 剩余秒数  
                };  
            })  
            .filter(order => order !== null); // 过滤掉null  
        pendingOrders.value = orders.value
            .filter(order => order.state === 1)
            .map(order => {  
                // 计算离过期时间  
                const orderCreationTime = new Date(order.orderTimestamp).getTime();
                const currentTime = new Date().getTime();  
                const timeDiff = (currentTime - orderCreationTime-8*60*60*1000);  
                // 如果超过30分钟，返回null，后面会通过filter删除  
                if (timeDiff > 30 * 60 * 1000){
                    return null;}
                // 添加倒计时属性  
                return {   
                    ...order,   
                    countdown: Math.max(0, (30 * 60 * 1000 - timeDiff) / 1000)  // 剩余秒数  
                };  
            })  
            .filter(order => order!== null); // 过滤掉null  
        deliveringOrders.value = orders.value.filter(order => order.state === 2);
        completedOrders.value = orders.value.filter(order => order.state === 3);
        console.log('订单',orders.value);
        console.log('未支付订单',nopayedOrders.value);
        console.log('待处理订单',pendingOrders.value);
        console.log('派送中订单',deliveringOrders.value);
        console.log('已完成订单',completedOrders.value);
    }catch(error){
        if (error.response && error.response.data) {  
            const errorCode = error.response.data.errorCode;  
            if (errorCode === 40000) {  
                ElMessage.error('无订单');  
            }
        }
    }
}
// 实现倒计时  
const updateCountdowns = () => {  
    for (const order of nopayedOrders.value) {  
        if (order.countdown > 0) {  
            order.countdown -= 1; // 每秒减少1  
        }  
    }  
    for(const order of pendingOrders.value) {  
        if (order.countdown > 0) {  
            order.countdown -= 1; // 每秒减少1  
        }  
    }
    // 刷新未支付订单列表，删除已经过期的订单  
    nopayedOrders.value = nopayedOrders.value.filter(order => order.countdown > 0);  
    pendingOrders.value = pendingOrders.value.filter(order => order.countdown > 0);  
};  

setInterval(updateCountdowns, 1000); // 每秒更新倒计时  
const enterOrderInfo = async(order) => {
    // console.log('新订单默认评价：',newOrderComment.value);
    currentOrder.value = order;
    const merchantData = await getMerchantsInfo(order.dishes[0].merchantId);
    currentMerchant.value = merchantData.data;
    for (const dish of currentOrder.value.dishes) { 
        const dishData = await getDishInfo(dish.merchantId,dish.dishId);
        dish.dishInfo = dishData.data;
    };  
    if(currentOrder.value.coupon){
        const couponData=await GetCouponInfo(currentOrder.value.coupon.couponId);
        currentOrder.value.coupon.couponInfo=couponData.data;
    }
    console.log('当前订单',currentOrder.value);
    isOrderInfo.value = true;
}
const leaveOrderInfo = () => {
    isOrderInfo.value = false;
    currentOrder.value = {};
}
const cancelPurchase = () => {
    showPayDialog.value = false;
    paymentPassword.value = '';
    paymentError.value = '';
    isOrderInfo.value = false;
}
const enterPayOrder = () => {
    showPayDialog.value = true;
}
const confirmPurchase = async() => {
    if (!paymentPassword.value) {  
        paymentError.value = '支付密码不能为空'; // 提示用户输入密码  
        return;  
    }  
    if (paymentPassword.value!== correctPassword.value) {  
        paymentError.value = '支付密码错误'; // 提示用户输入错误的密码  
        return;  
    }  
    try {  
        const response=await PurchaseOrder(currentOrder.value.orderId);  // 调用后端接口购买订单  
        console.log("购买订单",response);
        ElMessage.success("订单支付成功");  
        renewOrders();  // 更新订单信息
        showPayDialog.value = false;  // 关闭支付弹出框
        paymentPassword.value = '';
        paymentError.value = '';
        isOrderInfo.value = false;
    } catch (error) {  
        if (error.response && error.response.data) {  
             const errorCode = error.response.data.errorCode;  

            if (errorCode === 20001) {  
                ElMessage.error('购买失败');  
            } else if (errorCode === 20000) {  
                ElMessage.error('余额不足');  
            } else {  
                 ElMessage.error('购买失败，请稍后再试');  
            }  
        } else {  
                ElMessage.error('网络错误，请重试');  
        }  
    }  
}

const validateField = (field) => {
  if (refForm.value) {
    refForm.value.validateField(field).catch(() => {
      console.log(`${field} 验证失败`);
    });
  }
};
const cancelOrder = async() => {
    try {  
        const response = await deleteOrder(currentOrder.value.orderId);  
        console.log("取消订单",response);
        ElMessage.success("订单取消成功");  
        renewOrders();  // 更新订单信息
        showPayDialog.value = false;  // 关闭支付弹出框
        paymentPassword.value = '';
        paymentError.value = '';
        isOrderInfo.value = false;
    } catch (error) {  
        ElMessage.error('取消订单失败，请稍后再试');  
    }  
}
function formatDateTime(time) { 
    const date = new Date(time); 
    if (isNaN(date.getTime())) { 
        return null; // 或者处理无效日期的逻辑  
    } 
    const year = date.getFullYear(); 
    const month = String(date.getMonth() + 1).padStart(2, '0'); // 月份从0开始  
    const day = String(date.getDate()).padStart(2, '0'); 
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0'); 
    const seconds = String(date.getSeconds()).padStart(2, '0'); 

    return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`; 
}
</script>

<template>
    <div class="content">
    <div v-if="!isOrderInfo">
        <h2 class="header">
            我的订单
            <button class="action-button" @click="gobackHome">返回</button>
        </h2>
        <div class="state-buttons">
            <button class="state-button" @click="showState=0">未支付</button>
            <button class="state-button" @click="showState=1">待处理</button>
            <button class="state-button" @click="showState=2">派送中</button>
            <button class="state-button" @click="showState=3">已完成</button>
        </div>
        <div v-if="showState===0">
            <el-scrollbar max-height="500px">
                <ul class="order-list">
                    <li v-for="(order,index) in nopayedOrders" :key="index" class="order-item">
                        <p>订单号：{{order.orderId}}</p>
                        <p>订单总价：{{order.price}}元</p>
                        <p>订单创建时间：{{ formatDateTime(order.orderTimestamp) }}</p>
                        <p>支付剩余时间:{{ Math.floor(order.countdown/60) }}:{{ Math.floor(order.countdown%60) }}</p>
                        <button class="enter-button" @click="enterOrderInfo(order)">></button>
                    </li>
                </ul>
            </el-scrollbar>
        </div>
        
        <div v-if="showState===1">
            <el-scrollbar max-height="500px">
                <ul class="order-list">
                    <li v-for="(order,index) in pendingOrders" :key="index" class="order-item">
                        <p>订单号：{{order.orderId}}</p>
                        <p>订单总价：{{order.price}}元</p>
                        <p>订单创建时间：{{ formatDateTime(order.orderTimestamp) }}</p>
                        <p>等待骑手接单:{{ Math.floor(order.countdown/60) }}:{{ Math.floor(order.countdown%60) }}</p>
                        <button class="enter-button" @click="enterOrderInfo(order)">></button>
                    </li>
                </ul>
            </el-scrollbar>
        </div>
        
        <div v-if="showState===2">
            <el-scrollbar max-height="500px">

                <ul class="order-list">
                    <li v-for="(order,index) in deliveringOrders":key ="index" class="order-item">

                        <p>订单号：{{order.orderId}}</p>
                        <p>订单总价：{{ order.price }}元</p>
                        <p>订单创建时间：{{ formatDateTime(order.orderTimestamp) }}</p>
                        <button class="enter-button" @click="enterOrderInfo(order)">></button>
                    </li>
                </ul>
            </el-scrollbar>
        </div>
        
        <div v-if="showState===3">
            
            <!-- <ul>
                <li v-for="(order,index) in completedOrders":key ="inkey">
                    <p>订单号：{{order.orderId}}</p>
                    <p>订单总价：{{ order.price }}元</p>
                    <p>订单创建时间：{{ order.orderTimestamp }}</p>
                    <button @click="enterOrderInfo(order)">></button>
                </li>
            </ul> -->
            <el-scrollbar max-height="500px">

                <ul class="order-list">
                <li v-for="(order,index) in completedOrders":key ="index" class="order-item">

                    <p>订单号：{{order.orderId}}</p>
                    <p>订单总价：{{ order.price }}元</p>
                    <p>订单创建时间：{{ formatDateTime(order.orderTimestamp) }}</p>
                    <button class="enter-button" @click="enterOrderInfo(order)">></button>
                </li>
            </ul>
            </el-scrollbar>
        </div>

    </div>
    <div v-else>
        <h2 class="header">订单详情</h2>
        <p>订单号：{{currentOrder.orderId}}</p>
        <p>状态：{{ currentOrder.state===0?'未支付':currentOrder.state===1?'待处理':currentOrder.state===2?'派送中':currentOrder.state===3?'已完成':'未知状态' }}</p>
        <div class="address-info">
            <p>{{ currentOrder.address.contactName }}&nbsp;&nbsp;{{ currentOrder.address.phoneNumber }}</p>
             {{ currentOrder.address.userAddress }}&nbsp;{{ currentOrder.address.houseNumber}}
        </div>
        <p>{{currentMerchant.merchantName}}：</p>
        <p>联系电话：{{ currentMerchant.contact }}</p>
        <p>商家地址：{{ currentMerchant.merchantAddress }}</p>
        <ul class="dish-list">
            <li v-for="(dish,index) in currentOrder.dishes" :key="index" class="dish-item">
                <p><img :src="dish.dishInfo.imageUrl" alt="菜品图片"  class="dish-image">
                    {{ dish.dishInfo.dishName }} ×{{dish.dishNum}}
                </p>
            </li>
        </ul>
        <p>优惠券：{{currentOrder.coupon?currentOrder.coupon.couponInfo.couponName:'无'}} &nbsp;{{currentOrder.coupon?'满'+currentOrder.coupon.couponInfo.minPrice+'减'+currentOrder.coupon.couponInfo.couponValue+'元':''}}</p>
        <p>总价：{{currentOrder.price}}元</p>
        <div class="order-actions">
            <button class="action-button" @click="leaveOrderInfo()">返回</button>
            <button v-if="currentOrder.state===0" class="action-button" @click="enterPayOrder()">支付</button>
            <button v-if="currentOrder.state===0" class="action-button" @click="cancelOrder()">取消订单</button>
        </div>

        <!--添加评价表单-->
        <div v-if="currentOrder.state===3" class="comment-section">
            <h3>添加评价</h3>
            <el-form :rules="newOrderComment" ref="refForm" :model="currentOrder">
                <el-form-item label="商家评分" prop="merchantRating"><input v-model="currentOrder.merchantRating" placeholder="商家评分" @blur="validateField('merchantRating')"/></el-form-item> 
                <el-form-item label="骑手评分" prop="riderRating"><input v-model="currentOrder.riderRating" placeholder="骑手评分" @blur="validateField('riderRating')"/></el-form-item>   
                <el-form-item label="评价" prop="comment"><input v-model="currentOrder.comment" placeholder="评价" /></el-form-item>   
            </el-form>
            <button class="action-button" @click="submitComment(currentOrder.orderId)">提交评价</button>
        </div>

    </div>
     <!-- 弹窗 -->  
     <el-dialog title="输入支付密码" :model-value="showPayDialog" @close="cancelPurchase()">  
        <el-input   
            type="password"   
            v-model="paymentPassword"   
            placeholder="请输入6位支付密码"   
            clearable   
        />  
        <div v-if="paymentError"  class="payment-error">{{ paymentError }}</div>  
        <template #footer>  
            <el-button @click="cancelPurchase()">取消</el-button>  
            <el-button type="primary" @click="confirmPurchase()">确认</el-button>  
        </template>  
    </el-dialog>  
    </div>
</template>

<style scoped lang="scss">
.content {
  padding: 20px;
  background-color: transparent;
  margin-left: 120px; /* 考虑侧边栏的宽度 */
}

.header {
  font-size: 24px;
  font-weight: bold;
  margin-bottom: 20px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.state-buttons {
  padding: 10px;
  border-radius: 25px;
  display: flex;
  justify-content: space-between;
  background-color: #dda0dd !important; /* 强制应用粉色背景 */
  width: auto; /* 去掉 fit-content, 使用 auto */
  margin-bottom: 20px;
}

.state-button {
  padding: 10px 20px;
  margin-right: 10px;
  border-radius: 20px;
  background-color: #dda0dd;
  color: #fff;
  border: none;
  cursor: pointer;
  transition: background-color 0.3s ease-in-out;
}

.state-button:hover {
  background-color: #d8bfd8;
}


.order-list {
  padding: 0;
  list-style: none;
}

.order-item {
  padding: 15px;
  background-color: #fff;
  margin-bottom: 10px;
  border-radius: 10px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.enter-button {
  padding: 8px 12px;
  background-color: #dda0dd;
  color: #fff;
  border-radius: 50%;
  border: none;
  cursor: pointer;
  transition: background-color 0.3s ease-in-out;
}

.enter-button:hover {
  background-color: #d8bfd8;
}

.dish-list {
  list-style: none;
  padding: 0;
}

.dish-item {
  margin-bottom: 10px;
}

.dish-image {
  width: 50px;
  height: 50px;
  margin-right: 10px;
}

.address-info {
  margin-bottom: 20px;
}

.order-actions {
  margin-top: 20px;
}

.action-button {
  padding: 10px 15px;
  background-color: #dda0dd;
  color: #fff;
  border-radius: 25px;
  border: none;
  cursor: pointer;
  transition: background-color 0.3s ease-in-out;
}

.action-button:hover {
  background-color: #d8bfd8;
}

.comment-section {
  margin-top: 30px;
}

.payment-error {
  color: red;
}
</style>
