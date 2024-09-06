<script setup>
import { ref,onMounted,onUnmounted,inject,watch,computed,provide} from 'vue';
import { useRouter,onBeforeRouteLeave } from 'vue-router'
import { searchDishes, GetSpecialOffer } from '@/api/merchant';
import {addToShoppingCart, getMerchantIds, getMerchantsInfo, getShoppingCartinMerchant,decrementDishInCart,removeFromShoppingCart } from '@/api/user';
import { useStore } from "vuex";
import { ElMessage } from 'element-plus';


const store = useStore();  
const router = useRouter();
const user = ref({}); // 初始化用户信息对象
const MerchantId=ref(null);  //商户id
const merchantIds=ref([]);  //商户id列表
const MerchantInfo=ref({});  //商户信息
const dishes=ref([]);  //菜品列表
const showDishes=ref([]); //显示的菜品列表
const searchQuery = ref('');  // 搜索关键字
const merchantsInfo=inject('merchantsInfo');
const cartItems=ref([]);  //购物车物品列表
const specialOffers=ref([]);  //商家满减活动
const isMenu=ref(true);  //是否是菜单页面
const isOffersExpanded = ref(false); // 控制满减内容的展开和收起

onMounted (async() => {
  //获取用户信息
  if(router.currentRoute.value.path.includes('/order'))
    isMenu.value = false;
  else
    isMenu.value = true; 
  const userData = store.state.user; 
  if (userData) {  
    user.value = userData;  
  } else {  
    router.push('/login');
  }  
  const storedMerchantsInfo = localStorage.getItem('MerchantInfo');  //长效保存
  if (storedMerchantsInfo) {  
    merchantsInfo.value = JSON.parse(storedMerchantsInfo);  
  }
  
  getMerchantIds().then(res => {  // 获取所有商家id
    merchantIds.value = res.data;  
    return Promise.all(merchantIds.value.map(id => getMerchantsInfo(id))); // 并发请求所有商家信息
  }).then(responses => {  
    merchantsInfo.value = responses.map(response => response.data); // 提取商家信息  
  }).catch(err => {  
    ElMessage.error('获取商家id失败'); 
  }); 

  MerchantId.value=router.currentRoute.value.params.id;
  // MerchantInfo.value=merchantsInfo.value[MerchantId.value-1];
  MerchantInfo.value=merchantsInfo.value.find(item=>item.merchantId===Number(MerchantId.value));
  MerchantInfo.value.timeforOpenBusiness=formatTime(MerchantInfo.value.timeforOpenBusiness);  //时间转换
  MerchantInfo.value.timeforCloseBusiness=formatTime(MerchantInfo.value.timeforCloseBusiness);

  const data = await searchDishes(MerchantId.value);  
  if (data) {  
    dishes.value = data.data; // 假设后端返回的菜品数据是一个数组
    dishes.value = dishes.value.filter(dish => dish.dishInventory> 0);  //过滤出库存大于0的菜品
    showDishes.value=dishes.value;
  }  

  const shoppingCartData = await getShoppingCartinMerchant(user.value.userId, MerchantId.value);
  if (shoppingCartData) {  
    //console.log(shoppingCartData);  
    cartItems.value = shoppingCartData.data; // 假设后端返回的菜品数据是一个数组
  }  

  const offerData = await GetSpecialOffer(MerchantId.value); 
  if (offerData) {  
      specialOffers.value = offerData.data; // 假设后端返回的offer数据是一个数组
  }  


  // 添加鼠标滚轮监听器
  window.addEventListener('wheel', handleScroll);
});
onUnmounted(() => {
      // 在组件卸载时移除监听器
      window.removeEventListener('wheel', handleScroll);
});
const gobackHome = () => {
  router.push('/user-home');
}

// 切换展开/收起状态
const toggleOffersExpand = () => {
  isOffersExpanded.value = !isOffersExpanded.value;
}

// 观察 merchantsInfo 的变化，并保存到本地存储  
watch(merchantsInfo, (newValue) => {  
  localStorage.setItem('MerchantInfo', JSON.stringify(newValue));  
});  

const formatTime=(seconds)=> {  
  const hours = Math.floor(seconds / 3600);  
  const minutes = Math.floor((seconds % 3600) / 60);  
  const secs = seconds % 60;  
  return `${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}:${String(secs).padStart(2, '0')}`;  
}  

// 计算属性：搜索过滤  
const filteredDishes = computed(() => {  
  if (!searchQuery.value) {  
    return dishes.value; // 如果没有搜索关键字，返回所有菜品  
  }  
  return dishes.value.filter(dish => {  
    const nameMatch = dish.dishName.toLowerCase().includes(searchQuery.value.toLowerCase());  
    const categoryMatch = dish.dishCategory.toLowerCase().includes(searchQuery.value.toLowerCase());  
    return nameMatch || categoryMatch; // 根据名字或类别进行匹配  
  });  
});  

// 搜索函数  
const handleSearch = () => {  
  showDishes.value = filteredDishes.value; // 更新显示的菜品列表为过滤结果  
}  

// 加入购物车函数
const addToCart = async(dish) => {
    const userId = user.value.userId;
    
    // 调用API，将购物车项保存到数据库    
    try {
      const shoppingCartItem = {
        UserId: userId,  // 用户ID
        //ShoppingCartId: /* 会自动生成购物车ID */,
        MerchantId: MerchantId.value,  // 当前商户ID
        DishId: dish.dishId,  // 菜品ID
        DishNum: 1  // 默认加入1个
      };

      const response = await addToShoppingCart(shoppingCartItem); // 调用 API 创建收藏商户  
      ElMessage.success(response.msg); // 显示成功消息 
      //console.log(response); 

      // 更新前端数据中的商品数量
      const foundIndex = cartItems.value.findIndex(item => item.dishId === dish.dishId);
      if (foundIndex !== -1) {
          // 重新赋值以触发视图更新
          cartItems.value[foundIndex] = {
              ...cartItems.value[foundIndex],
              dishNum: cartItems.value[foundIndex].dishNum + 1
          };
      }
      else { // 若为新加入的菜品，创建一条记录
        // 更新购物车物品列表
        cartItems.value.push({
          dishId: shoppingCartItem.DishId,
          dishName: dish.dishName,
          dishNum: 1,
          dishPrice: dish.dishPrice,
          imageUrl: dish.imageUrl,
          merchantId:Number(MerchantId.value),
          merchantName:MerchantInfo.value.merchantName,
          shoppingCartId:response.data.shoppingCartId
        });
        //console.log(cartItems);
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
        const foundItem = cartItems.value.find(item => item.dishId === dish.dishId);
        if (foundItem) {
            foundItem.dishNum -= 1;
            // 如果数量减至0，可以选择将其从购物车移除
            if (foundItem.dishNum <= 0) {
              cartItems.value = cartItems.value.filter(item => item.dishId !== dish.dishId);
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
        cartItems.value = cartItems.value.filter(item => item.dishId !== dish.dishId);
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

// 控制购物车展开和收回的状态
const cartExpanded = ref(false);

// 切换购物车展开状态的函数
const toggleCart = () => {
  cartExpanded.value = !cartExpanded.value;
};
// 监听 cartItems 的变化，当购物车为空时将 cartExpanded 设置为 false
watch(cartItems, (newVal) => {
  if (newVal.length === 0) {
    cartExpanded.value = false;
  }
});

// 计算总价函数
const totalPrice = computed(() => {
    return cartItems.value.reduce((total, item) => {
        return total + item.dishPrice * item.dishNum;
    }, 0);
});

const discountAmount = computed(() => calculateDiscount(totalPrice.value, specialOffers));
const finalPrice = computed(() => {return totalPrice.value - discountAmount.value});
 
// 计算满减优惠的函数
function calculateDiscount(cartTotal, specialOffers) {
    // 过滤掉不满足条件的优惠
    const applicableOffers = specialOffers.value.filter(offer => cartTotal >= offer.minPrice);
    //console.log(applicableOffers);

    // 如果没有适用的优惠，返回0
    if (applicableOffers.length === 0) {
        return 0;
    }

    // 找到最大可用的优惠金额
    const maxDiscount = Math.max(...applicableOffers.map(offer => offer.amountRemission));

    return maxDiscount;
}
const gotoOrder = () => {  
  if(cartItems.value.length === 0){
    ElMessage.error('购物车为空，请先添加商品');
    return;
  }
  console.log('跳转到订单页面');  
  console.log(MerchantId.value);  
  isMenu.value = false; // 确保菜单不显示  
  router.push({   
    path: `/user-home/merchant/${MerchantId.value}/order`,   
  });  
};  
watch(  
  () => router.currentRoute.value.path,  
  (newPath) => {  
    isMenu.value = !newPath.includes('/order');  
  }  
);  

// 监听滚动事件的处理器
const handleScroll = (event) => {
      const delta = event.deltaY || event.detail || event.wheelDelta;
      // 实现滚动的逻辑，可以控制页面的垂直滚动
      document.querySelector('.content').scrollBy(0, delta); // 使内容区域进行滚动
};
</script>

<template>
  <div v-if="MerchantInfo && MerchantInfo.merchantName && isMenu" class="content" id="app">
    <header class="merchant-header">{{ MerchantInfo.merchantName }} 的菜单</header>
    
    <div class="merchant-info">
      <div>地址：{{ MerchantInfo.merchantAddress }}</div>
      <div>营业时间：{{ MerchantInfo.timeforOpenBusiness }} - {{ MerchantInfo.timeforCloseBusiness }}</div>
      <div>联系电话：{{ MerchantInfo.contact }}</div>
      <div>是否可以使用通用优惠券：{{ MerchantInfo.couponType ? '否' : '是' }}</div>
    </div>

    <div v-if="specialOffers.length" class="offers-section">
      <button class="offers-button" @click="toggleOffersExpand">
        {{ isOffersExpanded ? '收起满减活动' : '正在进行满减活动' }}
      </button>
      <div v-if="isOffersExpanded" class="offers-list">
        <ul>
          <li v-for="offer in specialOffers" :key="offer.offerId" class="offer-item">
            满 {{ offer.minPrice }} 元&nbsp;&nbsp;减 {{ offer.amountRemission }} 元
          </li>
        </ul>
      </div>
    </div>
    <div v-else>
      <p class="no-offers">暂无满减活动</p>
    </div>

    <div class="search-section">
      <el-col :span="8">
        <el-input placeholder="搜索菜品或类别" v-model="searchQuery" clearable @clear="handleSearch" @keydown.enter="handleSearch">
          <template #append>
            <el-button type="primary" @click="handleSearch" class="search-button">
              <el-icon><search /></el-icon>
            </el-button>
          </template>
        </el-input>
      </el-col>
    </div>

    <ul class="dish-list">
      <li v-for="dish in showDishes" :key="dish.dishId" class="dish-item">
        <img :src="dish.imageUrl" alt="菜品图片" class="dish-img">
        <div class="dish-info">
          <span>{{ dish.dishName }}：{{ dish.dishPrice }}元</span>
          <span>{{ dish.dishCategory }} &nbsp;&nbsp;库存：{{ dish.dishInventory }}</span>
        </div>
        <button @click="addToCart(dish)" class="add-cart-button">加入购物车</button>
      </li>
    </ul>

    <div v-if="cartItems.length > 0" class="cart-section">
      <button class="cart-button" @click="toggleCart">
        {{ cartExpanded ? '收回购物车' : '购物车' }}({{cartItems.length}})
      </button>
      <div v-if="cartExpanded" class="cart-items">
        <ul>
          <li v-for="item in cartItems" :key="item.dishId" class="cart-item">
            <img :src="item.imageUrl" alt="菜品图片" class="cart-img">
            <div class="cart-info">
              <span>{{ item.dishName }}: {{ item.dishPrice }}元</span>
              <div class="quantity-controls">
                <button @click="decrementInCart(item)" class="quantity-button">-</button>
                {{ item.dishNum }}
                <button @click="addToCart(item)" class="quantity-button">+</button>
              </div>
             </div>
            <button @click="removeInCart(item)" class="remove-button">x</button>
          </li>
        </ul>
      </div>
    </div>
    <div v-else class="empty-cart">购物车为空</div>

    <div class="total-section">
      <strong>总价: {{ finalPrice }} 元</strong>
      <span v-if="discountAmount != 0">({{ totalPrice }}-{{ discountAmount }})</span>
      <button @click="gotoOrder()" class="checkout-button">结算</button>
    </div>
    <router-view />
  </div>
  <router-view />
  <ChildComponent />
</template>

<style scoped>
html, body {
  height: 100%;
  margin: 0;
  padding: 0;
  font-family: 'Arial', sans-serif;
  overflow: hidden;  /* 确保全局允许滚动 */
}

#app {
  display: flex;
  flex-direction: column;
  height: 100%;
  overflow: hidden;
}

.content {
  flex-grow: 1;
  padding: 20px;
  background: linear-gradient(135deg, #f3f4f6, #fff);
  display: flex;
  flex-direction: column;
  height: calc(100vh - 20px); /* 计算总高度，确保超出可视窗口大小 */
  overflow-y: auto;
  justify-content: flex-start; /* 确保内容从顶部开始 */
  scrollbar-width: thin;
  background: transparent;
}

router-view {
  flex-grow: 1;
}

.merchant-header {
  font-size: 26px;
  font-weight: bold;
  color: #4A4A4A;
  text-align: center;
  margin-bottom: 20px;
}

.merchant-info {
  font-size: 16px;
  color: #555;
  margin-bottom: 20px;
  padding: 10px;
  border: 1px solid #ddd;
  border-radius: 8px;
  background-color: #f9f9f9;
}

.offers-section {
  margin-bottom: 20px;
}

.offers-button {
  background-color: #DDA0DD;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 25px;
  cursor: pointer;
  transition: background-color 0.3s ease;
}

.offers-button:hover {
  background-color: #D8BFD8;
}

.search-section {
  margin-bottom: 20px;
}

.search-button {
  background-color: #DDA0DD;
  border: none;
  color: #fff;
  border-radius: 50%;
  transition: background-color 0.3s ease;
}

.search-button:hover {
  background-color: #D8BFD8;
}

.dish-list {
  list-style: none;
  padding: 0;
  overflow-y: auto;
}

.dish-item {
  display: flex;
  align-items: center;
  margin-bottom: 15px;
  padding: 10px;
  border-bottom: 1px solid #ddd;
}

.dish-img {
  width: 50px;
  height: 50px;
  margin-right: 15px;
  border-radius: 8px;
  object-fit: cover;
}

.dish-info {
  flex-grow: 1;
}

.add-cart-button {
  background-color: #DDA0DD;
  border: none;
  color: white;
  padding: 8px 15px;
  border-radius: 25px;
  cursor: pointer;
  transition: background-color 0.3s ease;
}

.add-cart-button:hover {
  background-color: #D8BFD8;
}

.cart-section {
  margin-top: 20px;
}

.cart-button {
  background-color: #DDA0DD;
  border: none;
  color: white;
  padding: 10px 20px;
  border-radius: 25px;
  cursor: pointer;
  transition: background-color 0.3s ease;
}

.cart-button:hover {
  background-color: #D8BFD8;
}

.cart-items {
  margin-top: 20px;
}

.cart-item {
  display: flex;
  align-items: center;
  margin-bottom: 15px;
  padding: 10px;
  border-bottom: 1px solid #ddd;
}

.cart-img {
  width: 50px;
  height: 50px;
  margin-right: 15px;
  border-radius: 8px;
  object-fit: cover;
}

.cart-info {
  flex-grow: 1;
}

.quantity-controls {
  display: flex;
  align-items: center;
}

.quantity-button {
  background-color: #DDA0DD;
  border: none;
  color: white;
  padding: 6px 12px;
  border-radius: 50%;
  cursor: pointer;
  transition: background-color 0.3s ease;
}

.quantity-button:hover {
  background-color: #D8BFD8;
}

.remove-button {
  background-color: #ff6961;
  border: none;
  color: white;
  padding: 6px 12px;
  border-radius: 50%;
  cursor: pointer;
  transition: background-color 0.3s ease;
}

.remove-button:hover {
  background-color: #ff7f7f;
}

.empty-cart {
  margin-top: 20px;
  font-size: 16px;
  color: #888;
}

.total-section {
  margin-top: 20px;
  font-size: 18px;
}

.checkout-button {
  background-color: #DDA0DD;
  border: none;
  color: white;
  padding: 10px 20px;
  border-radius: 25px;
  cursor: pointer;
  margin-left: 100px;
  transition: background-color 0.3s ease;
}

.checkout-button:hover {
  background-color: #D8BFD8;
}
</style>
