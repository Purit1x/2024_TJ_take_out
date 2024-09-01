<script setup>
import { ref,onMounted, inject,watch,computed,provide} from 'vue';
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
  MerchantInfo.value=merchantsInfo.value[MerchantId.value-1];
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
</script>


<template>
    <div v-if="MerchantInfo && MerchantInfo.merchantName&& isMenu" class="content">
      <header>{{MerchantInfo.merchantName}}的菜单</header>
      <button @click="gobackHome()">&nbsp;&nbsp;返回主页</button>
      <div>地址：{{MerchantInfo.merchantAddress}}</div>
      <div>营业时间：{{ MerchantInfo.timeforOpenBusiness }} - {{ MerchantInfo.timeforCloseBusiness }}</div>
      <div>联系电话：{{MerchantInfo.contact}}</div>
      <div>是否可以使用通用优惠券：{{MerchantInfo.couponType ? '否' : '是'}}</div>

      <div v-if="specialOffers.length">
        <button @click="toggleOffersExpand">
          {{ isOffersExpanded ? '收起满减活动' : '正在进行满减活动' }}
        </button>
        <div v-if="isOffersExpanded">
          <ul>
              <li v-for="offer in specialOffers" :key="offer.offerId">
                  满 {{ offer.minPrice }} 元&nbsp;&nbsp;减 {{ offer.amountRemission }} 元
              </li>
          </ul>
        </div>
      </div>
      <div v-else>
        <p>暂无满减活动</p>
      </div>

      <div>
        <div>
          <input type="text" v-model="searchQuery" placeholder="搜索菜品或类别" v-on:keyup.enter="handleSearch()"/> 
          <button @click="handleSearch()">搜索</button>
        </div>
        <ul>
          <li v-for="dish in showDishes" :key="dish.dishId">
            <img :src="dish.imageUrl" alt="菜品图片" style="width: 50px; height: 50px;">
            {{dish.dishName}}：{{dish.dishPrice}}元 &nbsp;&nbsp;{{dish.dishCategory}} &nbsp;&nbsp;库存：{{dish.dishInventory}}
            <button @click="addToCart(dish)">加入购物车</button>
          </li>
        </ul>
      </div>

      <div v-if="cartItems.length > 0">
        <button @click="toggleCart">{{ cartExpanded ? '收回购物车' : '购物车' }}（{{cartItems.length}}）</button>
        <div v-if="cartExpanded">
          <ul>
            <li v-for="item in cartItems" :key="item.dishId">
              <img :src="item.imageUrl" alt="菜品图片" style="width: 50px; height: 50px;">
              {{item.dishName}}：{{item.dishPrice}}元 &nbsp;&nbsp;
              <button @click="decrementInCart(item)">-</button>
              {{item.dishNum}}  <!-- 显示商品数量 -->
              <button @click="addToCart(item)">+</button>
              <button @click="removeInCart(item)">x</button>
            </li>
          </ul>
        </div>
      </div>
      <div v-else>购物车为空</div>

      <div>
        <strong>总价: {{ finalPrice }} 元</strong><span v-if="discountAmount != 0">({{ totalPrice }}-{{ discountAmount }})</span>
        <button @click="gotoOrder()">结算</button>
      </div>
      <router-view />
    </div>
    <router-view />
    <ChildComponent /> 
</template>