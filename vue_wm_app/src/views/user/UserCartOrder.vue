<script setup>
import { useStore } from "vuex";
import { cloneDeep } from 'lodash'; // 如果使用 lodash
import { ElMessage } from "element-plus";
import { ref, onMounted, watch,inject,computed } from 'vue';  
import { useRouter } from 'vue-router';
import { GetDefaultAddress ,GetAddressByAddressId,getShoppingCartinMerchant,getAddressService,getUserCoupons,GetCouponInfo,CreateOrder,removeFromShoppingCart,userInfo,PurchaseOrder} from "@/api/user";
import {GetSpecialOffer,merchantInfo,getDistanceBetweenAddresses} from "@/api/merchant";
import { deleteShoppingCart } from "../../api/user";

const store = useStore();
const router = useRouter();

const UserId=ref({});
const DefaultAddressId=ref({});  //默认地址id
const hasDefaultAddress=ref(true);  //是否有默认地址
const DefaultAddressInfo=ref({});  //默认地址信息
const Addresses=ref([]);  //用户的所有地址
const ModifyingMerchant = ref(-1);//代表正在修改地址或其他信息的订单所属的商家

const coupons=ref([]);  //用户拥有的所有优惠券
const availableCoupons=ref([]);  //可用的优惠券

const showAddressDialog=ref(false);  //是否显示地址选择弹出框
const showCouponDialog=ref(false);  //是否显示优惠券选择弹出框

const CouponType=ref(0);  //优惠券类型,0表示仅支持通用优惠券

const paymentPassword=ref('');  //支付密码
const paymentError=ref('');  //支付密码错误提示
const showPayDialog=ref(false);  //是否显示订单创建成功弹出框
const correctPassword=ref('');  //正确的支付密码

// 从 Vuex 中获取 checkedItems
const checkedItems = computed(() => store.state.checkedItems);
const groupedItems = ref({});//各个商家的物品
const groupedPrice = ref({});//各个商家的总价
const groupedDiscountAmount=ref({});  //满减优惠金额

const merchantInfos=ref({});  //商家信息
const specialOffers=ref({});  //商家满减活动

const isNeedUtensils=ref({});  //是否需要餐具
const packetPrice=ref({});  //包装费
const riderPrice=ref({});  //骑手配送费


const choosedAddress = ref({});  //用户选择的各个商家对应的地址
const choosedCoupon=ref({});  //用户选择的优惠券

const orderIds = ref([]);  // 保存多个订单id

const FinalPrice = computed(() => {
    let tprice = 0;  // 使用 let 代替 const
    for (const merchantId in groupedItems.value) {
      let price = groupedPrice.value[merchantId] - groupedDiscountAmount.value[merchantId] + packetPrice.value[merchantId] + riderPrice.value[merchantId];  // 计算基础价格减去折扣
      if (choosedCoupon.value[merchantId]) {
        price -= choosedCoupon.value[merchantId].couponValue;  // 使用优惠券应减少价格
      }
      tprice += price;  // 累加每个商家的价格
    }
    return tprice;
});

const merchantFinalPrice = (merchantId)=>{
  let price =groupedPrice.value[merchantId] - groupedDiscountAmount.value[merchantId] + packetPrice.value[merchantId] + riderPrice.value[merchantId] -
        (choosedCoupon.value[merchantId] ? choosedCoupon.value[merchantId].couponValue : 0)
  return price;
}

onMounted(async() => {
    
    UserId.value=store.state.user.userId;
    try{
        const userInformation=await userInfo(UserId.value);  // 获取用户信息
        correctPassword.value=userInformation.data.walletPassword;  // 正确的支付密码
        //console.log('进入usercart页面');
        //console.log("购物车：",checkedItems.value);

        // 按商家分组购物车物品
        groupItemsByMerchant();
        console.log("grouped购物车",groupedItems.value);
        // 计算每一个商家的总价
        calculateGroupedPrice();
        //console.log("grouped price",groupedPrice.value);

        // 获取各个商家的信息
        groupedMerchantInfo();
        console.log('merchantInfo',merchantInfos.value);

        // 获取每个商家的满减优惠信息(没写函数)
        for (const merchantId in groupedItems.value) {
            const offerData = await GetSpecialOffer(merchantId);
            if (offerData) {
                specialOffers.value[merchantId] = offerData.data;
            }
        }
        //console.log('满减活动：',specialOffers.value);

        // 计算各个商家的满减优惠金额
        for (const merchantId in groupedItems.value) {
            const offerDiscount = calculateDiscount(merchantId);
            groupedDiscountAmount.value[merchantId] = offerDiscount;
        }
        console.log('offerDiscount:',groupedDiscountAmount.value);

        // 初始化所有商家的餐具包装费与配送费选项
        for (const merchantId in groupedItems.value) {
          isNeedUtensils.value[merchantId] = false;  //是否需要餐具
          packetPrice.value[merchantId] = 2;  //包装费
          riderPrice.value[merchantId] = 0;  //骑手配送费
        }

        const response = await GetDefaultAddress(UserId.value);  // 获取默认地址
        const addes=await getAddressService(UserId.value);  //获取用户地址
        Addresses.value=addes;
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
            //初始化每个商家对应的地址
            for (const merchantId in groupedItems.value) {
              choosedAddress.value[merchantId]=DefaultAddressInfo.value;
            }
            console.log('address:',choosedAddress);
        }

        // 处理优惠券
        const userCoupons=await getUserCoupons(UserId.value);  //获取用户拥有的优惠券
        //console.log(userCoupons);
        if(userCoupons){
          coupons.value=userCoupons.data;
          for(const coupon of coupons.value){
            const couponData=await GetCouponInfo(coupon.couponId);
            coupon.couponId=couponData.data.couponId;
            coupon.couponName=couponData.data.couponName;
            coupon.couponValue=couponData.data.couponValue;
            coupon.couponType=couponData.data.couponType;
            coupon.minPrice=couponData.data.minPrice;
          }
        }
        
        console.log('coupons: ',coupons.value);
        //console.log(availableCoupons.value);
      
    }catch(err){
        ElMessage.error(err.message);
    }
});

// 按商家分组物品
const groupItemsByMerchant = () => {
    const items = checkedItems.value;
    const grouped = {};

    items.forEach(item => {
        if (!grouped[item.merchantId]) {
            grouped[item.merchantId] = [];
        }
        grouped[item.merchantId].push(item);
    });

    groupedItems.value = grouped;
};

// 按组获取商家信息
const groupedMerchantInfo = async() => {
  merchantInfos.value = {};

  // 遍历每个商家的商品列表
  for (const merchantId in groupedItems.value) {
      const merchantInformation=await merchantInfo(merchantId);
      // 将商家信息储存在在对应商家的ID下
      merchantInfos.value[merchantId] = merchantInformation.data;
}
};

// 按商家分组计算总价
function calculateGroupedPrice() {
  groupedPrice.value = {};

  // 遍历每个商家的商品列表
  for (const merchantId in groupedItems.value) {
    const items = groupedItems.value[merchantId];
    let totalPrice = 0;

    // 遍历该商家的每个商品，计算总价
    items.forEach(item => {
      if (item.checked) { // 如果商品已选中
        totalPrice += item.dishNum * item.dishPrice;
      }
    });

    // 将计算的总价存储在对应商家的ID下
    groupedPrice.value[merchantId] = totalPrice;
  }
}

// 计算满减金额
const calculateDiscount = (merchantId) => {
    const totalPrice = groupedPrice.value[merchantId];
    //console.log('totalPrice: ',merchantId,totalPrice)
    const applicableOffers = specialOffers.value[merchantId].filter(offer => totalPrice >= offer.minPrice);

    //console.log('满减：',merchantId,applicableOffers);
    if (applicableOffers.length === 0) {
        return 0;
    }

    // 找到最大的折扣金额
    const maxDiscount = Math.max(...applicableOffers.map(offer => offer.amountRemission));
    return maxDiscount;
};

// 选择地址的处理函数  
const selectAddress = (address, merchantId) => {  
    //console.log('merchantId:',ModifyingMerchant.value);
    choosedAddress.value[ModifyingMerchant.value] = address; // 更新选择的地址  
    //console.log('debug address:',choosedAddress.value[ModifyingMerchant.value]);
    showAddressDialog.value = false; // 关闭弹出框
    ModifyingMerchant.value = -1;//还原正在修改的订单index  
};  

watch(choosedAddress, async (newAddresses, oldAddresses) => {
    for (const merchantId in newAddresses) {
        const newAddress = newAddresses[merchantId];
        //console.log('debug address2:',choosedAddress.value[merchantId]);
        if (newAddress && newAddress.userAddress) {
            // 获取商家的地址，例如从 groupedItems 中获取
            const merchantAddress = merchantInfos.value[merchantId].merchantAddress; // 假设你有每个商家的地址存储
            const distance = await getDistanceBetweenAddresses(merchantAddress, newAddress.userAddress);
            console.log(`商家 ${merchantId} 距离:`, distance);

            // 计算骑手配送费
            riderPrice.value[merchantId] = 2 + distance * 0.5;

            // 计算最终应付金额
            //FinalPrice.value[merchantId] = MerchantPrice.value[merchantId] + packetPrice.value[merchantId] + riderPrice.value[merchantId];
        } else {
            // 如果地址未设置，骑手配送费为 0
            riderPrice.value[merchantId] = 0;
            //FinalPrice.value[merchantId] = MerchantPrice.value[merchantId] + packetPrice.value[merchantId];
        }
    }
}, { deep: true });

// 过滤出可用的优惠券
const filterCoupon = () => {
  // 清空可用优惠券列表
  availableCoupons.value[ModifyingMerchant.value] = [];

  // 筛选符合条件的优惠券，并进行深拷贝
  availableCoupons.value[ModifyingMerchant.value] = coupons.value
    .filter(coupon => {
      return groupedPrice.value[ModifyingMerchant.value] >= coupon.minPrice && 
             !isCouponExpired(coupon.expirationDate) && 
             coupon.amountOwned > 0;  // 排除数量为0的优惠券
    })
    .map(coupon => cloneDeep(coupon));  // 使用 lodash 的 cloneDeep 进行深拷贝

  // 是否仅仅允许通用优惠券
  CouponType.value = merchantInfos.value[ModifyingMerchant.value].couponType;
  if (CouponType.value === 0) {
    availableCoupons.value[ModifyingMerchant.value] = availableCoupons.value[ModifyingMerchant.value]
      .filter(coupon => coupon.couponType === 0);
  }

  if (availableCoupons.value[ModifyingMerchant.value]) {
    for (const coupon of availableCoupons.value[ModifyingMerchant.value]) {
      for (const merchantId in groupedItems.value) {
        if (choosedCoupon.value[merchantId] && choosedCoupon.value[merchantId].couponId === coupon.couponId) {
          coupon.amountOwned -= 1;
          break;
        }
      }
    }
  }
};

// 选择一个优惠券
const selectCoupon = (coupon) => {  
  // 如果没有选择过优惠券或者选择的优惠券不同
  if (!choosedCoupon.value[ModifyingMerchant.value] || coupon.couponId !== choosedCoupon.value[ModifyingMerchant.value].couponId) {
    // 调整之前选择的优惠券数量
    /*if (choosedCoupon.value[ModifyingMerchant.value]) {
      const previousCoupon = choosedCoupon.value[ModifyingMerchant.value];
      for (const c of coupons.value) {
        if (c.couponId === previousCoupon.couponId) {
          c.amountOwned += 1;  // 恢复之前优惠券的数量
          break;
        }
      }
    }*/

    // 更新选择的优惠券
    choosedCoupon.value[ModifyingMerchant.value] = coupon; 

    // 减少新选择的优惠券数量
    /*for (const c of coupons.value) {
      if (c.couponId === coupon.couponId) {
        c.amountOwned -= 1;  // 减少新选择的优惠券数量
        break;
      }
    }*/
  }

  showCouponDialog.value = false;  // 关闭弹出框
  ModifyingMerchant.value = -1;    // 重置商家
};

// 取消当前使用的优惠券
const noCoupon = () => {  
  // 恢复之前选择的优惠券数量
  /*if (choosedCoupon.value[ModifyingMerchant.value]) {
    const previousCoupon = choosedCoupon.value[ModifyingMerchant.value];
    for (const c of coupons.value) {
      if (c.couponId === previousCoupon.couponId) {
        c.amountOwned += 1;  // 恢复优惠券数量
        break;
      }
    }
  }*/

  // 取消选择优惠券
  choosedCoupon.value[ModifyingMerchant.value] = null; 

  showCouponDialog.value = false;  // 关闭弹出框
  ModifyingMerchant.value = -1;    // 重置商家
};
// 检查优惠券是否过期的辅助函数  
function isCouponExpired(expirationDate) {  
    const now = new Date();  
    return new Date(expirationDate) < now; // 如果过期时间在当前时间之前，返回 true  
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


const submitOrder = async () => { 
    //checkTime();  // 检查是否在营业时间内
    for (const merchantId in groupedItems.value){
      if (choosedAddress.value[merchantId].addressId===undefined) {   //检查是否选择了地址
        ElMessage.error("请选择地址");  
        return;  
      }
    }
    
    for (const merchantId in groupedItems.value){
      const orderData = {  
          Price:merchantFinalPrice(merchantId),  
          OrderTimestamp:new Date().toISOString(),  
          NeedUtensils:isNeedUtensils.value[merchantId]?1:0,  
          AddressId:choosedAddress.value[merchantId].addressId,  
          CouponId:choosedCoupon.value[merchantId] ? choosedCoupon.value[merchantId].couponId : 0,
          ExpirationDate:choosedCoupon.value[merchantId] ? choosedCoupon.value[merchantId].expirationDate : new Date().toISOString(),  
          MerchantId:merchantInfos.value[merchantId].merchantId,
          shoppingCart:groupedItems.value[merchantId],  
          UserId: UserId.value,
          RiderPrice: riderPrice.value[merchantId]
      };  
      console.log("提交订单数据",orderData);
      try {  
          const response = await CreateOrder(orderData);  
          orderIds.value.push(response.data);  // 保存订单id
          ElMessage.success("订单创建成功");  
          
          //const dsc=await deleteShoppingCart(UserId.value, MerchantId.value);  
          showPayDialog.value = true;  // 显示支付弹出框
          console.log("清空购物车");
      } catch (error) {  
          if (error.response && error.response.data) {  
            const errorCode = error.response.data.errorCode;  
            if (errorCode === 20001) {  
                ElMessage.error(error.response.data.msg);  
            }
          }
      }
    }
    console.log("订单创建成功:",orderIds.value)
    try{
      // 清空购物车
      deleteItemsFromCart();
      console.log("清空购物车");
    }
    catch(error){
      if (error.response && error.response.data) {  
          const errorCode = error.response.data.errorCode;  
          if (errorCode === 20001) {  
              ElMessage.error(error.response.data.msg);  
          }
      }
    }
}

// 将所有下单的物品从购物车中删除
const deleteItemsFromCart = async () => {
    const items = checkedItems.value;


    for (const item of items) {
        const cartItem = {
          UserId: UserId.value,
          DishId: item.dishId,
          DishNum: 1
        };
        const msg = await removeFromShoppingCart(cartItem);
        console.log(msg);  // 可以输出结果进行检查
    }
};

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
        for(const orderId of orderIds.value){
          const response=await PurchaseOrder(orderId);  // 调用后端接口购买订单  
          console.log("购买订单",response);
        }
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


</script>

<template>
  <div class="content">
    <button class="action-button" @click="gobackHome">返回</button>
    <div v-for="(items, merchantId) in groupedItems" :key="merchantId" class="merchant-order">
      <h2 class="order-header">
        {{ merchantInfos[merchantId].merchantName }} 的订单
      </h2>

      <!-- 地址部分 -->
      <div class="address-section">
        <div class="address-info">
          联系人与地址：&nbsp;&nbsp;
          <button class="action-button" @click="showAddressDialog=true; ModifyingMerchant=merchantId">更换</button>
        </div>
        <div v-if="choosedAddress[merchantId].contactName">
          {{ choosedAddress[merchantId].contactName }}&nbsp;&nbsp;
          {{ choosedAddress[merchantId].phoneNumber }}&nbsp;&nbsp;
          <div>
            {{ choosedAddress[merchantId].userAddress }}&nbsp;&nbsp;
            {{ choosedAddress[merchantId].houseNumber }}&nbsp;&nbsp;
          </div>
        </div>
        <div v-else >
          <div v-if="!hasDefaultAddress">尚未设置默认地址，请选择地址。</div>
          <div v-else>
            {{ DefaultAddressInfo[merchantId].contactName }}&nbsp;&nbsp;
            {{ DefaultAddressInfo[merchantId].phoneNumber }}&nbsp;&nbsp;
            <div>
              {{ DefaultAddressInfo[merchantId].userAddress }}&nbsp;&nbsp;
              {{ DefaultAddressInfo[merchantId].houseNumber }}&nbsp;&nbsp;
            </div>
          </div>
        </div>
      </div>

      <!-- 订单详情部分 -->
      <div class="order-details">
        <p>订单：</p>
        <ul>
          <li v-for="item in items" :key="item.dishId" class="order-item">
            <img :src="item.imageUrl" alt="菜品图片" class="dish-image">
            {{ item.dishName }}：{{ item.dishPrice }}元 &nbsp;&nbsp;×{{ item.dishNum }}
          </li>
        </ul>
        <!-- 商家总价 -->
        <strong>总价: {{ groupedPrice[merchantId] }} 元</strong>
        <span v-if="discountAmount != 0">({{ groupedPrice[merchantId] }}-{{ groupedDiscountAmount[merchantId] }})</span>
      </div>

      <!-- 优惠券部分 -->
      <div class="coupon-section">
        优惠券：
        {{ choosedCoupon[merchantId] ? choosedCoupon[merchantId].couponName + " " + choosedCoupon[merchantId].couponValue + "元" : "无" }}
        <button class="action-button" @click="ModifyingMerchant=merchantId;filterCoupon(); showCouponDialog=true;">更换</button>
      </div>

      <div>打包费：{{ packetPrice[merchantId] }}元</div>
      <div>骑手配送费：{{ choosedAddress[merchantId].userAddress ? riderPrice[merchantId]+"元" : "请选择地址" }}</div>

      <!-- 应付金额 -->
      <strong>应付金额: {{ groupedPrice[merchantId] - groupedDiscountAmount[merchantId] + packetPrice[merchantId] + riderPrice[merchantId] -
        (choosedCoupon[merchantId] ? choosedCoupon[merchantId].couponValue : 0) }} 元</strong>

      <div class="utensils-section">
        <label>
          <input type="checkbox" v-model="isNeedUtensils[merchantId]" />需要餐具
        </label>
      </div>
    </div>

    <!-- 总计金额 -->
    <strong>总计金额: {{ FinalPrice }} 元</strong>

    <!-- 提交订单按钮 -->
    <div class="submit-order">
      <button class="submit-button" @click="submitOrder(merchantId)">提交订单</button>
    </div>

    <!-- 地址选择弹出框 -->
    <el-dialog title="选择地址" :model-value="showAddressDialog" width="600px" @close="showAddressDialog=false">
      <div>
        <ul>
          <li v-for="address in Addresses" :key="address.id" @click="selectAddress(address)">
            {{ address.contactName }} - {{ address.userAddress }} {{ address.houseNumber }}
            <button class="select-button" @click="selectAddress(address)">选择</button>
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
          <li v-for="coupon in availableCoupons[ModifyingMerchant]" :key="coupon.couponId" @click="selectCoupon(coupon)">
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
