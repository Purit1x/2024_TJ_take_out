<script setup>
import { useStore } from "vuex";
import { ElMessage } from "element-plus";
import { ref, onMounted, watch,inject,computed } from 'vue';  
import { useRouter } from 'vue-router';
import { GetDefaultAddress ,GetAddressByAddressId,getShoppingCartinMerchant,getAddressService,getUserCoupons,GetCouponInfo,CreateOrder,deleteShoppingCart,userInfo,PurchaseOrder} from "@/api/user";
import {GetSpecialOffer,merchantInfo,getDistanceBetweenAddresses} from "@/api/merchant";

const store = useStore();
const router = useRouter();
const MerchantId=ref({});
const UserId=ref({});
const DefaultAddressId=ref({});  //默认地址id
const hasDefaultAddress=ref(true);  //是否有默认地址
const DefaultAddressInfo=ref({});  //默认地址信息
const shoppingCart=ref([]);  //购物车
const specialOffers=ref([]);  //商家满减活动
const coupons=ref([]);  //用户拥有的所有优惠券
const Addresses=ref([]);  //用户的所有地址
const choosedAddress=ref({});  //用户选择的地址
const showAddressDialog=ref(false);  //是否显示地址选择弹出框
const showCouponDialog=ref(false);  //是否显示优惠券选择弹出框
const isCouponChoosed =ref(false);  //是否选择了优惠券
const choosedCoupon=ref({});  //用户选择的优惠券
const totalPrice=ref(0);  //总价
const discountAmount=ref(0);  //满减优惠金额
const MerchantPrice=ref(0);  //商家应付价格
const FinalPrice=ref(0);  //最终价格
const CouponType=ref(0);  //优惠券类型,0表示仅支持通用优惠券
const isNeedUtensils=ref(false);  //是否需要餐具
const packetPrice=ref(2);  //包装费
const riderPrice=ref(0);  //骑手配送费
const merchantAddress=ref({});  //商家地址
const start=ref(0);  //商家营业开始时间
const end=ref(0);  //商家营业结束时间
const paymentPassword=ref('');  //支付密码
const paymentError=ref('');  //支付密码错误提示
const showPayDialog=ref(false);  //是否显示订单创建成功弹出框
const correctPassword=ref('');  //正确的支付密码
const orderId=ref(0);  //订单id
onMounted(async() => {
    MerchantId.value = router.currentRoute.value.params.id; // 从路径参数中获取MerchantId
    UserId.value=store.state.user.userId;
    try{
        const merchantInformation=await merchantInfo(MerchantId.value);  // 获取商家信息
        const userInformation=await userInfo(UserId.value);  // 获取用户信息
        merchantAddress.value=merchantInformation.data.merchantAddress;  // 商家地址
        correctPassword.value=userInformation.data.walletPassword;  // 正确的支付密码
        console.log("商家信息",merchantInformation.data);

        start.value=merchantInformation.data.timeforOpenBusiness;  //检查是否在营业时间内
        end.value=merchantInformation.data.timeforCloseBusiness-30*60;  //营业结束前30分钟
        checkTime();

        const response = await GetDefaultAddress(UserId.value);  // 获取默认地址

        const sc=await getShoppingCartinMerchant(UserId.value, MerchantId.value);  // 获取购物车
        shoppingCart.value=sc.data;
        console.log("购物车",sc);

        const offerData = await GetSpecialOffer(MerchantId.value);   //获取商家满减
        if (offerData) {  
            specialOffers.value = offerData.data; // 假设后端返回的offer数据是一个数组
            console.log("商家满减活动",specialOffers.value)
        }  
        const addes=await getAddressService(UserId.value);  //获取用户地址
        Addresses.value=addes;
        
        //决定商家满减金额
        totalPrice.value=getTotalPrice();
        discountAmount.value=calculateDiscount(totalPrice.value, specialOffers);
        MerchantPrice.value=totalPrice.value-discountAmount.value;

        const userCoupons=await getUserCoupons(UserId.value);  //获取用户拥有的优惠券
        coupons.value=userCoupons.data;
        for(const coupon of coupons.value){
            const couponData=await GetCouponInfo(coupon.couponId);
            coupon.couponId=couponData.data.couponId;
            coupon.couponName=couponData.data.couponName;
            coupon.couponValue=couponData.data.couponValue;
            coupon.couponType=couponData.data.couponType;
            coupon.minPrice=couponData.data.minPrice;
        }
        coupons.value=coupons.value.filter(coupon => {  //筛选符合条件的优惠券
            return totalPrice.value >= coupon.minPrice && !isCouponExpired(coupon.expirationDate);});  
        //是否仅仅允许通用优惠券
        CouponType.value=merchantInformation.data.couponType;
        if(CouponType.value===0){
            coupons.value=coupons.value.filter(coupon => coupon.couponType===0);
        }
        choosedCoupon.value=await selectBestCoupon();
        if(choosedCoupon.value){
            FinalPrice.value=MerchantPrice.value-choosedCoupon.value.couponValue;
        }else{
            FinalPrice.value=MerchantPrice.value;
        }
        console.log("选择的最佳优惠券",choosedCoupon.value);
        console.log("用户地址",Addresses.value);

        //获取默认地址
        if(response.data==='none')
        {
            hasDefaultAddress.value=false;
            
        }
        else
        {
            DefaultAddressId.value=response.data;
            hasDefaultAddress.value=true;
            const DAInfo=await GetAddressByAddressId(DefaultAddressId.value);
            DefaultAddressInfo.value=DAInfo.data;
            choosedAddress.value=DefaultAddressInfo.value;
        }
    }catch(err){
        ElMessage.error(err.message);
    }
});
// 监听 choosedAddress 的变化  
watch(choosedAddress, async(newAddress, oldAddress) => {  
    // 在这里，所以你可以重新计算最终价格  
    if (newAddress && newAddress.userAddress) {  
        const distance = await getDistanceBetweenAddresses(merchantAddress.value, newAddress.userAddress);  
        console.log("距离", distance);  
        riderPrice.value = 2 + distance * 0.5;  // 骑手配送费按距离的千米数计算，底价为2  
        FinalPrice.value = MerchantPrice.value + packetPrice.value + riderPrice.value;  
    } else {  
        riderPrice.value = 0;  
        FinalPrice.value = MerchantPrice.value + packetPrice.value;  
    }  
});  

const checkTime = () => {   
    const nowDate = new Date();      
    const nowTime = changeDateToInt(nowDate);  
    if (start.value < end.value) {  
        // 正常营业时间  
        if (nowTime < start.value || nowTime > end.value) {  
            ElMessage.error("商家暂停营业");  
            router.push('/user-home');  
        }  
    } else {  
        // 跨越午夜的营业时间  
        if (nowTime < start.value && nowTime > end.value) {   
            ElMessage.error("商家暂停营业");  
            router.push('/user-home');  
        }  
    }  
}  
const changeDateToInt = (input) => {
    const date = new Date(input);
    const Hour = date.getHours();  
    const Minute = date.getMinutes();  
    const Second = date.getSeconds();  
    return Hour * 3600 + Minute * 60 + Second;  
}
const gobackHome = () => {
    router.push(`/user-home/merchant/${MerchantId.value}`);
}
const getTotalPrice = () => {
    return shoppingCart.value.reduce((total, item) => {
        return total + item.dishPrice * item.dishNum;
    }, 0);
}
// 计算满减优惠的函数
function calculateDiscount(cartTotal, specialOffers) {
    // 过滤掉不满足条件的优惠
    const applicableOffers = specialOffers.value.filter(offer => cartTotal >= offer.minPrice);

    // 如果没有适用的优惠，返回0
    if (applicableOffers.length === 0) {
        return 0;
    }
    // 找到最大可用的优惠金额
    const maxDiscount = Math.max(...applicableOffers.map(offer => offer.amountRemission));
    console.log("最大优惠金额",maxDiscount);

    return maxDiscount;
}
// 选择地址的处理函数  
const selectAddress = (address) => {  
    choosedAddress.value = address; // 更新选择的地址  
    showAddressDialog.value = false; // 关闭弹出框  
};  
const selectCoupon = (coupon) => {  
    choosedCoupon.value = coupon; // 更新选择的优惠券  
    FinalPrice.value=MerchantPrice.value-choosedCoupon.value.couponValue+packetPrice.value+riderPrice.value;
    showCouponDialog.value = false; // 关闭弹出框  
    isCouponChoosed.value=true;
};  
const noCoupon = () => {  
    // choosedCoupon.value = null; // 取消选择优惠券  
    FinalPrice.value=MerchantPrice.value+packetPrice.value+riderPrice.value;
    showCouponDialog.value = false; // 关闭弹出框  
    isCouponChoosed.value=false;
};  
const selectBestCoupon=async()=>{
    // 过滤出符合条件的优惠券  
    const validCoupons = coupons.value;
    // 如果没有符合条件的优惠券，返回 null  
    if (validCoupons.length === 0) {  
        return null;  
    }  
    // 找到最大减免金额  
    const maxDiscountValue = Math.max(...validCoupons.map(coupon => coupon.couponValue));  
    // 过滤出减免金额等于最大减免金额的优惠券  
    const maxDiscountCoupons = validCoupons.filter(coupon => coupon.couponValue === maxDiscountValue);  
    // 从最大减免金额的优惠券中找到距离过期时间最近的优惠券  
    const closestToExpirationCoupon = maxDiscountCoupons.reduce((closestCoupon, currentCoupon) => {  
        return new Date(currentCoupon.expirationDate) < new Date(closestCoupon.expirationDate) ? currentCoupon : closestCoupon;  
    });  
    return closestToExpirationCoupon;  
}
// 检查优惠券是否过期的辅助函数  
function isCouponExpired(expirationDate) {  
    const now = new Date();  
    return new Date(expirationDate) < now; // 如果过期时间在当前时间之前，返回 true  
}  
const submitOrder = async () => {  
    checkTime();  // 检查是否在营业时间内
    if (choosedAddress.value.addressId===undefined) {   //检查是否选择了地址
        ElMessage.error("请选择地址");  
        return;  
    }
    const orderData = {  
        Price:FinalPrice.value,  
        OrderTimestamp:new Date().toISOString(),  
        NeedUtensils:isNeedUtensils.value?1:0,  
        AddressId:choosedAddress.value.addressId,  
        CouponId:choosedCoupon.value ? choosedCoupon.value.couponId : 0,
        ExpirationDate:choosedCoupon.value ? choosedCoupon.value.expirationDate : new Date().toISOString(),  
        MerchantId:MerchantId.value,
        shoppingCart:shoppingCart.value,  
        UserId: UserId.value,
        RiderPrice: riderPrice.value
    };  
    console.log("提交订单数据",orderData);
    try {  
        const response = await CreateOrder(orderData);  
        orderId.value=response.data;  // 保存订单id
        ElMessage.success("订单创建成功");  
        // 清空购物车
        const dsc=await deleteShoppingCart(UserId.value, MerchantId.value);  
        showPayDialog.value = true;  // 显示支付弹出框
        console.log("清空购物车",dsc);
    } catch (error) {  
        if (error.response && error.response.data) {  
            const errorCode = error.response.data.errorCode;  
            if (errorCode === 20001) {  
                ElMessage.error(error.response.data.msg);  
            }
        }
    }
}
const cancelPurchase = () => {
    showPayDialog.value = false;
    paymentPassword.value = '';
    paymentError.value = '';
    router.push(`/user-home`);  // 跳转到商家页面
}
const confirmPurchase = async () => {  
    if (!paymentPassword.value) {  
        paymentError.value = '支付密码不能为空'; // 提示用户输入密码  
        return;  
    }  
    if (paymentPassword.value!== correctPassword.value) {  
        paymentError.value = '支付密码错误'; // 提示用户输入错误的密码  
        return;  
    }  
    try {  
        const response=await PurchaseOrder(orderId.value);  // 调用后端接口购买订单  
        console.log("购买订单",response);
        ElMessage.success("订单支付成功");  
        showPayDialog.value = false;  // 关闭支付弹出框
        paymentPassword.value = '';
        paymentError.value = '';
        router.push(`/user-home`);  // 跳转到商家页面
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
};  
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
    <div>
      <h2 class="order-header">
        下单
        <el-button class="action-button" @click="gobackHome">返回</el-button>
      </h2>
      <div class="address-section">
        <div class="address-info" style="display: flex;justify-content:space-between;align-items: center;">
          联系人与地址：
          <el-button class="action-button" @click="showAddressDialog=true">更换</el-button>
        </div>
        <div v-if="hasDefaultAddress" class="default-address">
          {{ choosedAddress.contactName }}&nbsp;&nbsp;
          {{ choosedAddress.phoneNumber }}&nbsp;&nbsp;
          <div>
            {{ choosedAddress.userAddress }}&nbsp;&nbsp;
            {{ choosedAddress.houseNumber }}&nbsp;&nbsp;
          </div>
        </div>
        <div v-if="!hasDefaultAddress" class="no-address">
          <div v-if="!choosedAddress.contactName">尚未设置默认地址，请选择地址。</div>
          <div v-else>
            {{ choosedAddress.contactName }}&nbsp;&nbsp;
            {{ choosedAddress.phoneNumber }}&nbsp;&nbsp;
            <div>
              {{ choosedAddress.userAddress }}&nbsp;&nbsp;
              {{ choosedAddress.houseNumber }}&nbsp;&nbsp;
            </div>
          </div>
        </div>
      </div>
      <div class="order-details">
        订单：
        <ul>
          <li v-for="item in shoppingCart" :key="item.dishId" class="order-item">
            <img :src="item.imageUrl" alt="菜品图片" class="dish-image">
            {{ item.dishName }}：{{ item.dishPrice }}元 &nbsp;&nbsp;×{{ item.dishNum }}
          </li>
        </ul>
        <strong>总价: {{ MerchantPrice }} 元</strong>
        <span v-if="discountAmount != 0">({{ totalPrice }}-{{ discountAmount }})</span>
      </div>
      <div class="coupon-section" style="display: flex;justify-content:space-between;align-items: center;">
        优惠券：
        {{ isCouponChoosed ? (choosedCoupon.couponName + " " + choosedCoupon.couponValue + "元"): (choosedCoupon ? "有可用优惠券" : "无可用优惠券") }}
        <!-- {{ choosedCoupon ? choosedCoupon.couponName + " " + choosedCoupon.couponValue + "元" : "无" }} -->
        <el-button class="action-button" @click="showCouponDialog=true">更换</el-button>
      </div>
      <div>打包费：{{ packetPrice }}元</div>
      <div>骑手配送费：{{ choosedAddress.userAddress ? riderPrice+"元" : "请选择地址" }}</div>
      <strong>应付金额: {{ FinalPrice }} 元</strong>
      <div class="utensils-section">
        <label>
          <input type="checkbox" v-model="isNeedUtensils" />需要餐具
        </label>
      </div>
      <div class="submit-order">
        <button class="submit-button" @click="submitOrder()">提交订单</button>
      </div>
    </div>

    <!-- 地址选择弹出框 -->
    <el-dialog title="选择地址" :model-value="showAddressDialog" width="600px" @close="showAddressDialog=false">
      <div>
        <ul>
          <li v-for="address in Addresses" :key="address.id" @click="selectAddress(address)">
            {{ address.contactName }} - {{ address.userAddress }} {{ address.houseNumber }}
            <el-button class="select-button" @click="selectAddress(address)">选择</el-button>
          </li>
        </ul>
      </div>
      <span class="dialog-footer">
        <el-button @click="showAddressDialog = false">取消</el-button>
      </span>
    </el-dialog>

    <!-- 优惠券选择弹出框 -->
    <el-dialog title="选择优惠券" :model-value="showCouponDialog" width="800px" @close="showCouponDialog=false">
      <div>
        <ul>
          <li v-for="coupon in coupons" :key="coupon.couponId" @click="selectCoupon(coupon)">
            {{ coupon.couponName }} 满{{coupon.minPrice}}减{{ coupon.couponValue }}元 过期时间：{{ formatDateTime(coupon.expirationDate) }}
            &nbsp;&nbsp;&nbsp;×{{ coupon.amountOwned }}
            <button class="select-button" @click="selectCoupon(coupon)">选择</button>
          </li>
        </ul>
      </div>
      <span class="dialog-footer">
        <el-button @click="noCoupon()">取消使用优惠券</el-button>
      </span>
    </el-dialog>

    <!-- 支付弹窗 -->
    <el-dialog title="输入支付密码" :model-value="showPayDialog" @close="cancelPurchase()">
      <el-input type="password" v-model="paymentPassword" placeholder="请输入6位支付密码" clearable />
      <div v-if="paymentError" class="payment-error">{{ paymentError }}</div>
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
  background-color: #f9f9f9;
  margin-left: 120px; /* 给侧边栏留出空间 */
}

.order-header {
  font-size: 24px;
  font-weight: bold;
  margin-bottom: 20px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.address-section,
.order-details,
.coupon-section {
  margin-bottom: 20px;
  padding: 10px;
  background-color: #fff;
  border-radius: 10px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.dish-image {
  width: 50px;
  height: 50px;
}

.action-button {
  padding: 10px 15px;
  border-radius: 25px;
  background-color: #dda0dd;
  border: none;
  color: #fff;
  cursor: pointer;
  transition: background-color 0.3s ease-in-out;
}

.action-button:hover {
  background-color: #d8bfd8;
}

.submit-button {
  width: 100%;
  padding: 15px;
  font-size: 18px;
  background-color: #dda0dd;
  color: #fff;
  border-radius: 30px;
  border: none;
  cursor: pointer;
  transition: background-color 0.3s ease-in-out;
}

.submit-button:hover {
  background-color: #d8bfd8;
}

.select-button {
  padding: 5px 10px;
  border-radius: 20px;
  background-color: #dda0dd;
  color: #fff;
  cursor: pointer;
  transition: background-color 0.3s;
}

.select-button:hover {
  background-color: #d8bfd8;
}

.utensils-section {
  margin-bottom: 20px;
}

.payment-error {
  color: red;
}
</style>
