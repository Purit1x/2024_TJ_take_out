<script setup>
import { useRouter } from 'vue-router'
import { ref, onMounted, onUnmounted, computed, watch } from 'vue';
import { addToShoppingCart, decrementDishInCart, removeFromShoppingCart, getShoppingCartItems } from '@/api/user';
import { GetMultiSpecialOffer } from '@/api/merchant';
import { useStore } from "vuex";
import { ElMessage } from 'element-plus';
import gsap from 'gsap';
const store = useStore();
const router = useRouter();
const user = ref({});
const merchants = ref([]);
const specialOffers = ref([]);

const isScrolling = ref(true);  // 滚动开关

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
      checked: false,
      dishes: merchant.dishes.map(dish => ({
        ...dish,
        checked: false,
      })),
    }));
  }

  const merchantIds = merchants.value.map(merchant => merchant.merchantId);
  const offers = await GetMultiSpecialOffer(merchantIds);
  if (offers) {
    specialOffers.value = offers.data;
  }

  const content = document.querySelector('.content');

  // 添加鼠标滚轮控制
  const handleScroll = (event) => {
    if (isScrolling.value) {
      content.scrollTop += event.deltaY;
    }
  };

  // 鼠标左键点击切换滚动状态
  const handleMouseClick = (event) => {
    if (event.button === 0) {
      isScrolling.value = !isScrolling.value;
    }
  };

  // 事件监听
  content.addEventListener('wheel', handleScroll);
  content.addEventListener('mousedown', handleMouseClick);

  // 在组件卸载时移除事件监听器
  onUnmounted(() => {
    content.removeEventListener('wheel', handleScroll);
    content.removeEventListener('mousedown', handleMouseClick);
  });
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
    <header class="header">我的购物车</header>
    <div v-for="merchant in merchants" :key="merchant.merchantId" class="cart-item">
      <div class="merchant-header">
        <input type="checkbox" class="checkbox" :checked="merchant.checked" @change="toggleMerchantSelection(merchant)" />
        <span class="merchant-name">{{ merchant.merchantName }}</span>
        <button class="merchant-button" @click="goToMerchantPage(merchant.merchantId)">></button>
      </div>
      <ul class="dish-list">
        <li v-for="item in merchant.dishes" :key="item.dishId" class="dish-item">
          <input type="checkbox" class="checkbox" :checked="item.checked" @change="toggleItemSelection(merchant, item)" />
          <img :src="item.imageUrl" alt="菜品图片" class="dish-img">
          <span class="dish-name">{{item.dishName}}</span>
          <span class="dish-price">{{item.dishPrice}}元</span>
          <div class="quantity-control">
            <button class="quantity-button" @click="decrementInCart(item)">-</button>
            <span class="quantity">{{item.dishNum}}</span>
            <button class="quantity-button" @click="addToCart(item)">+</button>
          </div>
          
          <button class="remove-button" @click="removeInCart(item)">x</button>
        </li>
      </ul>
    </div>

    <!-- 显示总价 -->
    <div class="total-price-container">
      <strong class="total-price">总价: {{ finalTotalPrice }} 元</strong>
      <span v-if="totalDiscount != 0" class="discount">({{ totalPrice }}-{{ totalDiscount }})</span>
    </div>
  </div>
</template>

<style scoped>
/* 页面整体样式 */
.content {
  padding: 20px;
  background-color: transparent;
  font-family: Arial, sans-serif;
  height: calc(100vh - 60px); /* 计算总高度，确保超出可视窗口大小 */
  overflow-y: auto; /* 强制显示垂直滚动条 */
  scrollbar-width: thin; /* 适用于 Firefox 的滚动条宽度设置 */
}

.content::-webkit-scrollbar {
  width: 8px; /* 滚动条宽度 */
}

.content::-webkit-scrollbar-track {
  background-color: #f9f9f9; /* 滚动条轨道颜色 */
}

.content::-webkit-scrollbar-thumb {
  background-color: #ff69b4; /* 滚动条颜色 */
  border-radius: 10px; /* 圆角滚动条 */
}

.content::-webkit-scrollbar-thumb:hover {
  background-color: #ff85c1; /* 悬停时滚动条的颜色 */
}

.header {
  font-size: 28px;
  font-weight: bold;
  color: #333;
  margin-bottom: 20px;
}

/* 商家标题样式 */
.merchant-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background-color: #DDA0DD;
  padding: 10px;
  border-radius: 8px;
  margin-bottom: 15px;
  color: white;
  margin-right: 20px;
}

.merchant-name {
  font-size: 20px;
  font-weight: bold;
}

.merchant-button {
  background-color: transparent;
  border: none;
  font-size: 20px;
  color: white;
  cursor: pointer;
  transition: color 0.3s;
}

.merchant-button:hover {
  background-color: #D8BFD8;
}

/* 菜品列表样式 */
.dish-list {
  list-style: none;
  padding: 0;
  margin-right: 20px; /* 添加右侧距离 */
}

.dish-item {
  display: flex;
  align-items: center;
  background-color: transparent;
  padding: 10px;
  border-radius: 8px;
  margin-bottom: 10px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  transition: box-shadow 0.3s;
  margin-right: 20px; /* 添加右侧距离 */
}

.dish-item:hover {
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
}

.checkbox {
  margin-right: 10px;
}

.dish-img {
  width: 50px;
  height: 50px;
  border-radius: 5px;
  margin-right: 10px;
}

.dish-name {
  flex: 1;
  font-size: 18px;
  color: #333;
}

.dish-price {
  font-size: 18px;
  font-weight: bold;
  color: #DDA0DD;
  margin-right: 20px;
}

.quantity-control {
  display: flex;
  align-items: center;
  gap: 10px;
  background-color: rgb(243, 235, 243);
}

.quantity-button {
  background-color: #dbb8db;
  border: none;
  color: white;
  padding: 5px 10px;
  border-radius: 20%;
  cursor: pointer;
  transition: background-color 0.3s;
}

.quantity-button:hover {
  background-color: #D8BFD8;
}

.quantity {
  font-size: 18px;
  color: #333;
}

.remove-button {
  background-color: transparent;
  border: none;
  color: #DDA0DD;
  font-size: 18px;
  cursor: pointer;
  transition: color 0.3s;
}

.remove-button:hover {
  color: #D8BFD8;
}

/* 总价样式 */
.total-price-container {
  margin-top: 20px;
  font-size: 22px;
  color: #333;
  display: flex;
  align-items: center;
}

.total-price {
  font-weight: bold;
}

.discount {
  margin-left: 10px;
  font-size: 18px;
  color: #DDA0DD;
}
</style>