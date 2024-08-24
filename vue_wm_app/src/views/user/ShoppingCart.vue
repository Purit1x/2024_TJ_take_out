<script setup>
import { useRouter } from 'vue-router'
import { ref,onMounted,computed } from 'vue';
import {addToShoppingCart,decrementDishInCart,removeFromShoppingCart,getShoppingCartItems} from '@/api/user';
import { useStore } from "vuex";
import { ElMessage } from 'element-plus';

const store = useStore();  
const router = useRouter();
const user = ref({}); // 初始化用户信息对象
const items=ref([]);  //购物车物品列表


onMounted(async() => {  
    //获取用户信息
    const userData = store.state.user; 
    if (userData) {  
        user.value = userData;  
    } else {  
        router.push('/login');
    }  
    const userId = user.value.userId;

    const data = await getShoppingCartItems(userId); 
    if (data) {  
        items.value = data.data; // 假设后端返回的菜品数据是一个数组
        items.value.forEach(item => item.checked = false); // 初始化时将所有的商品设为未勾选状态
    }  

})


const gobackHome = () => {
    router.push('/user-home');
}

// 加入购物车函数
const addToCart = async(dish) => {
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
        const foundIndex = items.value.findIndex(item => item.dishId === dish.dishId);
        if (foundIndex !== -1) {
            // 重新赋值以触发视图更新
            items.value[foundIndex] = {
                ...items.value[foundIndex],
                dishNum: items.value[foundIndex].dishNum + 1
            };
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
        const foundItem = items.value.find(item => item.dishId === dish.dishId);
        if (foundItem) {
            foundItem.dishNum -= 1;
            // 如果数量减至0，可以选择将其从购物车移除
            if (foundItem.dishNum <= 0) {
                items.value = items.value.filter(item => item.dishId !== dish.dishId);
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
        items.value = items.value.filter(item => item.dishId !== dish.dishId);
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

// 计算选中的商品
const checkedItems = computed(() => items.value.filter(item => item.checked));

// 计算选中商品的总价
const totalPrice = computed(() => {
    return checkedItems.value.reduce((total, item) => total + item.dishPrice * item.dishNum, 0);
});

</script>

<template>
    <div class="content">
      <header>我的购物车</header>
        <div class="cart-item">
            <ul> 
            <li v-for="item in items" :key="item.dishId" class="cart_item-li">
                <input type="checkbox" v-model="item.checked"/>
                <img :src="item.imageUrl" class="cart-item-img" alt="菜品图片">
                {{item.dishName}}：{{item.dishPrice}}元 &nbsp;&nbsp;{{item.merchantName}}
                <button @click="decrementInCart(item)">-</button>
                {{item.dishNum}}  <!-- 显示商品数量 -->
                <button @click="addToCart(item)">+</button>
                <button @click="removeInCart(item)">删除</button>
            </li>
            </ul>
        </div>

        <!-- 显示总价 -->
        <div>
            <strong>总价: {{ totalPrice }} 元</strong>
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