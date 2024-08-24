<script setup>  
import { ref, onMounted, watch } from 'vue';  
import { useRouter } from 'vue-router';
import { useStore } from "vuex";
import { provide } from 'vue';
import { getMerchantIds,getMerchantsInfo,createFavouriteMerchant } from "@/api/user";
import { ElMessage } from 'element-plus';

const store = useStore();    
const router = useRouter();
const user = ref({}); // 初始化用户信息对象  
const isUserHome = ref(true); // 页面是否为用户主页 
const merchantIds = ref([]);  //获取所有商家id
const merchantsInfo = ref([]); // 存储所有商家信息 
const isMenu= ref(false); // 是否在看菜单
const searchQuery = ref(''); // 搜索框内容
const showMerchantsInfo = ref([]); // 显示商家信息列表

onMounted(() => {  
  const userData = store.state.user; 
  if(router.currentRoute.value.path !== '/user-home')
    isUserHome.value = false;
  else
    isUserHome.value = true; 
  if (userData) {  
    user.value = userData;  
  } else {  
    router.push('/login');
  }  
  getMerchantIds().then(res => {  // 获取所有商家id
    merchantIds.value = res.data;  
    return Promise.all(merchantIds.value.map(id => getMerchantsInfo(id))); // 并发请求所有商家信息
  }).then(responses => {  
    merchantsInfo.value = responses.map(response => response.data); // 提取商家信息  
    showMerchantsInfo.value = merchantsInfo.value; // 显示商家信息列表
  }).catch(err => {  
    ElMessage.error('获取商家id失败'); 
  }); 
}); 

watch(  
    () => router.currentRoute.value.path,  
    (newPath) => {  
        // 检查当前路由是否是用户主页或商家菜单  
        if (newPath.startsWith('/user-home') &&   
            newPath !== '/user-home/personal'&&
            (newPath !== '/user-home/cart')&&
            (newPath !== '/user-home/address')&&
            (newPath !== '/user-home/personal/coupon')&&
            (newPath !== '/user-home/personal/coupon/couponPurchase')) {  
            isUserHome.value = !newPath.startsWith('/user-home/merchant/'); // 如果是商家菜单，设置为 false  
        } else {  
            isUserHome.value = false;   
        }   
        localStorage.setItem('isUserHome', isUserHome.value);  
    }  
);   
// 返回主页函数
const gobackHome = () => {
  router.push('/user-home');
};
// 跳转到个人信息  
const goToPersonal = () => { 
    router.push('/user-home/personal');  
    isUserHome.value = false; // 进入个人信息页面时隐藏欢迎信息和按钮  
};  
const enterDishes = (id) => {  
  isUserHome.value = false; // 进入商家详情页面时隐藏欢迎信息和按钮  
  router.push('/user-home/merchant/' + id);  
};  
const goToCart = () => {  
  isUserHome.value = false; // 进入购物车页面时隐藏欢迎信息和按钮  
  router.push('/user-home/cart');  
};  
const goToAddress = () => {  
  isUserHome.value = false; // 进入地址页面时隐藏欢迎信息和按钮  
  router.push('/user-home/address');  
};  
const addToFavorite = async (merchantId) => {  
    const userId = user.value.userId; // 从用户对象中获取用户 ID  
    try {  
        const data = {    
            UserId: userId,  
            MerchantId: merchantId  
        };  

        const response = await createFavouriteMerchant(data); // 调用 API 创建收藏商户  
        ElMessage.success(response.msg); // 显示成功消息  
    } catch (error) {  
      if (error.response && error.response.data) {  
        const errorCode = error.response.data.errorCode;  

        if (errorCode === 20000) {  
         ElMessage.error('没有更新数据');  
        } else if (errorCode === 30000) {  
          ElMessage.error('请勿重复收藏');  
          } else {  
            ElMessage.error('发生未知错误');  
            }  
        } else {  
          ElMessage.error('网络错误，请重试');  
        }  
    }  
};  
const handleSearch = () => {   //字符串匹配搜索商家
  const query = searchQuery.value.trim();  
  if (query) {  
    showMerchantsInfo.value = merchantsInfo.value.filter(merchant =>  
      merchant.merchantName.includes(query) ||  
      merchant.dishType.includes(query)  
    );  
  } else {  
    showMerchantsInfo.value = merchantsInfo.value;  
  }
};  
// 提供 user 对象 给其它子网页 
provide('user', user); 
provide('merchantsInfo', merchantsInfo); 
</script>  

<template>  
  <nav class="sidebar">
      <div class="sidebar-content">
        <img class="sidebar-img" src="@\assets\my_logo.png" alt="logo"/>
        
        <button class="sidebar-button" @click="gobackHome">
          <img src="@\assets\merchant_home.png" alt="主页"/>
          <span>主页</span>
        </button>
        
        <button class="sidebar-button" @click="goToCart">
          <img src="@\assets\user_order.png" alt="购物车"/>
          <span>购物车</span>
        </button>
        
        <button class="sidebar-button" @click="goToPersonal">
          <img src="@\assets\merchant_personal.png" alt="个人信息"/>
          <span>个人信息</span>
        </button>

        <button class="sidebar-button" @click="goToAddress">
          <img src="@\assets\address.png" alt="地址"/>
          <span>地址</span>
        </button>
        <router-view /> <!-- 渲染子路由 -->
      </div>
  </nav>

    <div class="content">  
        <div class="content-header">
          <h1 v-if="isUserHome" class="welcome-text">欢迎，{{user.userId}}</h1>  
        </div>
        <div v-if="isUserHome">  
            <div class="search-bar">
              <input type="text" v-model="searchQuery" placeholder="搜索店名或类别" v-on:keyup.enter="handleSearch()"/> 
              <button @click="handleSearch()">搜索</button>
            </div>
            <table>
              <tbody>
                <tr v-for="merchant in showMerchantsInfo" :key="merchant.merchantId">
                  <td class="col-name">{{ merchant.merchantName }}</td> 
                  <td class="col-type">{{ merchant.dishType }}</td>
                  <td class="col-enter"><button @click="enterDishes(merchant.merchantId)">></button></td>
                  <td class="col-favorite"><button @click="addToFavorite(merchant.merchantId)">收藏</button></td>
                </tr>
              </tbody>
            </table> 
        </div>   
    </div>  
</template>  

<style>
body{
  background: linear-gradient(to bottom right, #f4ebf5, #f38bd248);
  margin: 0;
  padding: 0;
  height: 100vh;
}
</style>

<style scoped>
table{
  width: 100%;
  border-collapse: collapse;
}

th, td {
  border: 1px solid #ddd;
  padding: 8px;
  text-align: center;
}

th {
  background-color: #7BA7AB;
  color: #fff;
  text-align: center;
}

.col-name{width:40%;}
.col-type{width:40%;}
.col-enter{width:10%;}
.col-favorite{width:10%;}

.sidebar {
  width: 50px;
  background: linear-gradient(to bottom, #f0d6f2, #f77dd048);
  padding: 20px;
  height: 100vh;
  position: fixed;
  top: 0;
  left: 0;
}

.sidebar-img {
  width: 100%;
  height: auto;
  margin-bottom: 15px;
}

.sidebar-button {
  display: block;
  width: 50px;
  height: auto;
  margin-bottom: 20px;
  padding: 0;
  text-align: center;
  border: none;
  background-color: transparent;
  cursor: pointer;
  img {
    width: 100%;
    height: auto;
  }
}

.sidebar-button span {
  display: block;
  font-size: 12px;
  text-align: center;
}

.sidebar-button.active {
  color: #0f628b;
}

.sidebar-content button:hover {
  background-color: #3686d748;
}

.welcome-text {
  font-size: 35px;
  margin-left: 15px;
}

.content {
  font-size: 15px;
}

/* #Q# 这里有bug 不知道为什么按钮总比旁边少一点高度，height都是100% */
.search-bar {
  width: 300px;
  height: 40px;
  display: flex;
  margin-bottom: 20px;
  margin-top: 5px;
}
.search-bar input {
    width: 75%;
    height: 100%;
    padding-left: 15px;
    border-radius: 5px 0 0 5px;
    border: 2px solid #7BA7AB;
    background: #F9F0DA;
    color: #9E9C9C;
    outline: none;
}
.search-bar button {
    width: 25%;
    height: 100%;
    background: #7BA7AB;
    border: 2px solid #7BA7AB;
    border-radius: 0 5px 5px 0;
    font-size: 13px;
    position: relative;
    text-align: center;
}
.search-bar button:before {
    font-size: 13px;
    color: #F9F0DA;
}
</style>