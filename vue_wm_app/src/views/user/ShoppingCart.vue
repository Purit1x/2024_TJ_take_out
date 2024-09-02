<script setup>
import { useRouter } from 'vue-router'
import { ref, onMounted, computed, watch } from 'vue';
import {addToShoppingCart,decrementDishInCart,removeFromShoppingCart,getShoppingCartItems} from '@/api/user';
import { GetMultiSpecialOffer } from '@/api/merchant';
import { useStore } from "vuex";
import { ElMessage } from 'element-plus';
import gsap from 'gsap';
const store = useStore();  
const router = useRouter();
const user = ref({}); // 初始化用户信息对象
const merchants = ref([]);  // 存储按商家分类后的购物车物品列表
const specialOffers=ref([]);  //商家满减活动


onMounted(async () => {
    const userData = store.state.user;
    if (userData) {
        user.value = userData;
    } else {
        router.push('/login');
    }

    const userId = user.value.userId;
    const data = await getShoppingCartItems(userId);
    if (data) {
        merchants.value = data.data.map(merchant => ({
            ...merchant,
            checked: false, // 初始化商家复选框
            dishes: merchant.dishes.map(dish => ({
                ...dish,
                checked: false, // 初始化菜品复选框
            }))
        }));
        //console.log(merchants.value);
    }

    // 获取所有商家的满减数据
    const merchantIds = merchants.value.map(merchant => merchant.merchantId);
    const offers = await GetMultiSpecialOffer(merchantIds);
    if(offers){
      specialOffers.value = offers.data;
    }
    
    //console.log(specialOffers.value); 
});

// 商家复选框函数
const toggleMerchantSelection = (merchant) => {
    const newCheckedStatus = !merchant.checked;
    merchant.checked = newCheckedStatus;
    merchant.dishes.forEach(item => item.checked = newCheckedStatus);
};

// 商品复选框函数
const toggleItemSelection = (merchant, item) => {
    item.checked = !item.checked;
    merchant.checked = merchant.dishes.every(item => item.checked);// 商店的所有商品都被勾选就将商店勾选
};

// 计算商家总价
const getMerchantTotalPrice = (merchant) => {
    return merchant.dishes
        .filter(item => item.checked) // 只计算勾选的商品
        .reduce((total, item) => total + (item.dishPrice * item.dishNum), 0);
};

// 计算满减金额
const calculateDiscount = (merchant) => {
    const totalPrice = getMerchantTotalPrice(merchant);
    const applicableOffers = specialOffers.value.filter(offer => offer.merchantId === merchant.merchantId && totalPrice >= offer.minPrice);

    if (applicableOffers.length === 0) {
        return 0;
    }

    // 找到最大的折扣金额
    const maxDiscount = Math.max(...applicableOffers.map(offer => offer.amountRemission));
    return maxDiscount;
};

// 计算勾选商品的总价和折扣
const checkedItems = computed(() => {
    return merchants.value.flatMap(merchant => merchant.dishes.filter(item => item.checked));
});

const totalPrice = computed(() => {
    return merchants.value.reduce((total, merchant) => total + getMerchantTotalPrice(merchant), 0);
});

const totalDiscount = computed(() => {
    return merchants.value.reduce((total, merchant) => total + calculateDiscount(merchant), 0);
});

const finalTotalPrice = computed(() => totalPrice.value - totalDiscount.value);


const gobackHome = () => {
    router.push('/user-home');
}

const goToMerchantPage = (merchantId) => {
    router.push('/user-home/merchant/' + merchantId);
};

// 加入购物车函数
const addToCart = async(dish,buttonElement) => {
    const userId = user.value.userId;

    // 调用API，将购物车项保存到数据库    
    try {
      const shoppingCartItem = {
        UserId: userId,  // 用户ID
        //ShoppingCartId: /* 会自动生成购物车ID */,
        //MerchantId: MerchantId.value,  // 当前商户ID
        DishId: dish.dishId,  // 菜品ID
        DishNum: 1  // 默认加入1个
      };

        const response = await addToShoppingCart(shoppingCartItem); // 调用 API 创建收藏商户  
        ElMessage.success(response.msg); // 显示成功消息  


        // 更新前端数据中的商品数量
        for (const merchant of merchants.value) {
            const foundDish = merchant.dishes.find(item => item.dishId === dish.dishId);
            if (foundDish) {
                foundDish.dishNum += 1;
                break;
            }
        }
    } catch (error) {  
      if (error.response && error.response.data) {  
        const errorCode = error.response.data.errorCode;  

        if (errorCode === 20000) {  
          ElMessage.error('添加失败');  
        } 
        else {  
          ElMessage.error('发生未知错误');  
        }  
      } 
      else {  
        console.info(error.toString());
        ElMessage.error('网络错误，请重试');  
      }  
    }  
};

// 删除n个购物车物品函数（n = 1）
const decrementInCart = async(dish) => {
    const userId = user.value.userId;

    // 调用API，将购物车项保存到数据库    
    try {
      const shoppingCartItem = {
        UserId: userId,  // 用户ID
        //MerchantId: MerchantId.value,  // 当前商户ID
        DishId: dish.dishId,  // 菜品ID
        DishNum: 1  // 默认删除1个
      };

        const response = await decrementDishInCart(shoppingCartItem); // 调用 API 创建收藏商户  
        ElMessage.success(response.msg); // 显示成功消息  

        // 更新前端数据中的商品数量
        for (const merchant of merchants.value) {
            const foundDish = merchant.dishes.find(item => item.dishId === dish.dishId);
            if (foundDish) {
                foundDish.dishNum -= 1;
                if (foundDish.dishNum <= 0) {
                    merchant.dishes = merchant.dishes.filter(item => item.dishId !== dish.dishId);
                }
                // 如果商店中的所有商品都被移除，则移除商店
                if (merchant.dishes.length === 0) {
                    merchants.value = merchants.value.filter(m => m.merchantId !== merchant.merchantId);
                }
                break;
            }
        }
    } catch (error) {  
      if (error.response && error.response.data) {  
        const errorCode = error.response.data.errorCode;  

        if (errorCode === 20000) {  
          ElMessage.error('删除失败');  
        } 
        else {  
          ElMessage.error('发生未知错误');  
        }  
      } 
      else {  
        ElMessage.error('网络错误，请重试');  
      }  
    }  
};

// 删除购物车项函数
const removeInCart = async(dish) => {
    const userId = user.value.userId;

    // 调用API，将购物车项保存到数据库    
    try {
      const shoppingCartItem = {
        UserId: userId,  // 用户ID
        //MerchantId: MerchantId.value,  // 当前商户ID
        DishId: dish.dishId,  // 菜品ID
        DishNum: 1
      };

        const response = await removeFromShoppingCart(shoppingCartItem); // 调用 API 创建收藏商户  
        ElMessage.success(response.msg); // 显示成功消息  

        // 更新前端，删除菜品
        for (const merchant of merchants.value) {
            if (merchant.dishes.some(item => item.dishId === dish.dishId)) {
                merchant.dishes = merchant.dishes.filter(item => item.dishId !== dish.dishId);
                // 如果商店中的所有商品都被移除，则移除商店
                if (merchant.dishes.length === 0) {
                    merchants.value = merchants.value.filter(m => m.merchantId !== merchant.merchantId);
                }
                break;
            }
        }
    } catch (error) {  
      if (error.response && error.response.data) {  
        const errorCode = error.response.data.errorCode;  

        if (errorCode === 20000) {  
          ElMessage.error('删除失败');  
        } 
        else {  
          ElMessage.error('发生未知错误');  
        }  
      } 
      else {  
        ElMessage.error('网络错误，请重试');  
      }  
    }  
};

</script>

<template>
    <div class="content">
      <header>我的购物车</header>
      <div v-for="merchant in merchants" :key="merchant.merchantId" class="cart-item">
          <input type="checkbox" :checked="merchant.checked" @change="toggleMerchantSelection(merchant)" />
          {{ merchant.merchantName }}
          <button @click="goToMerchantPage(merchant.merchantId)">></button>
          <ul>
              <li v-for="item in merchant.dishes" :key="item.dishId">
                  <input type="checkbox" :checked="item.checked" @change="toggleItemSelection(merchant, item)" />
                  <img :src="item.imageUrl" alt="菜品图片" style="width: 50px; height: 50px;">
                  {{item.dishName}}：{{item.dishPrice}}元
                  <button @click="decrementInCart(item)">-</button>
                  {{item.dishNum}}
                  <button @click="addToCart(item)">+</button>
                  <button @click="removeInCart(item)">x</button>
              </li>
          </ul>
      </div>

        <!-- 显示总价 -->
        <div>
          <div><strong>总价: {{ finalTotalPrice }} 元</strong><span v-if="totalDiscount != 0">({{ totalPrice }}-{{ totalDiscount }})</span></div>
        </div>
    </div>
</template>


<style scoped>
.cart-item ul{
  display: flex;
  flex-direction: column;
  font-size: 20px;
  gap: 15px;
}

.cart_item-li{
  display: flex;
  align-items: center;
  gap: 10px;
}

.cart-item-img {
    align-content: center;
    width: 60px; 
    height: 60px;
}

</style>