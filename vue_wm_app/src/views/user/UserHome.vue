<script setup>  
import { ref, onMounted, watch } from 'vue';  
import { useRouter } from 'vue-router';
import { useStore } from "vuex";
import { provide } from 'vue';
import { getMerchantIds,getMerchantsInfo,createFavouriteMerchant,GetDefaultAddress,GetUserAddress } from "@/api/user";
import {getDistanceBetweenAddresses} from "@/api/merchant";
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
const hasDefaultAddress = ref(true); // 用于跟踪是否有默认地址
const DefaultAddress=ref(null); // 用于跟踪默认地址
const DefaultAddressId=ref(null); // 用于跟踪默认地址id
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
    fetchDefaultAddress();  // 获取用户默认地址
    showMerchantsInfo.value = merchantsInfo.value; // 显示商家信息列表
  }).catch(err => {  
    ElMessage.error('获取商家id失败'); 
  });  
}); 
const fetchDefaultAddress = async () => {  
    try {  
        const res = await GetDefaultAddress(user.value.userId); // 获取用户默认地址  
        if (res.data !== 'none') {  
            hasDefaultAddress.value = true;  
            DefaultAddressId.value = res.data;  

            // 获取默认地址  
            const addressRes = await GetUserAddress(DefaultAddressId.value);  
            DefaultAddress.value = addressRes.data;  
            console.log(DefaultAddress.value);  

            if (hasDefaultAddress.value && DefaultAddress.value) {  
                // 计算每个商家与默认地址之间的距离  
                for (const merchant of merchantsInfo.value) {  
                    // 假设商家的地址为 merchant.merchantAddress  
                    const distance = await getDistanceBetweenAddresses(merchant.merchantAddress, DefaultAddress.value)/1000;  
                    // 将计算的距离添加到商家信息中，单位为米，并保留一位小数  
                    merchant.distanceFromDefaultAddress = distance ? (distance * 1000).toFixed(1) : 0;  
                }  
                merchantsInfo.value.sort((a, b) => {  //升序排列
                    const distanceA = parseFloat(a.distanceFromDefaultAddress) || 0;  
                    const distanceB = parseFloat(b.distanceFromDefaultAddress) || 0;  
                    return distanceA - distanceB;  
                });  
            }  
            console.log(merchantsInfo.value);  
            showMerchantsInfo.value = merchantsInfo.value; // 显示商家信息列表  
        } else {  
            hasDefaultAddress.value = false;  
            ElMessage.error('请设置默认地址');  
        }  
    } catch (err) {  
        ElMessage.error('请设置默认地址'); // 处理获取默认地址以及其他潜在错误  
        console.error(err); // 可选：记录错误以便调试  
    }  
};  
watch(  
    () => router.currentRoute.value.path,  
    (newPath) => {  
        // 检查当前路由是否是用户主页或商家菜单  
        fetchDefaultAddress();
        if (newPath.startsWith('/user-home') &&   
            newPath !== '/user-home/personal'&&
            (newPath !== '/user-home/cart')&&
            (newPath !== '/user-home/address')&&
            (newPath !== '/user-home/personal/coupon')&&
            (newPath !== '/user-home/personal/coupon/couponPurchase')&&
            (newPath !== '/user-home/personal/myOrder')) {  
            isUserHome.value = !newPath.startsWith('/user-home/merchant/'); // 如果是商家菜单，设置为 false  
        } else {  
            isUserHome.value = false;   
        }   
        localStorage.setItem('isUserHome', isUserHome.value);  
    }  
);   
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
    <div>  
        <h1 v-if="isUserHome">欢迎，{{user.userId}}</h1>  
        <button v-if="isUserHome" @click="goToPersonal">我的</button> 
        <button v-if="isUserHome" @click="goToCart">购物车</button>
        <button v-if="isUserHome" @click="goToAddress">地址</button>  
        <div v-if="isUserHome">  
            <h2>商家列表</h2>  
            <div>
              <input type="text" v-model="searchQuery" placeholder="搜索店名或类别" v-on:keyup.enter="handleSearch()"/> 
              <button @click="handleSearch()">搜索</button>
            </div>
            <ul>  
                <li v-for="merchant in showMerchantsInfo" :key="merchant.merchantId">  
                    <span>{{ merchant.merchantName }}</span> 
                    <span>&nbsp;&nbsp;{{ merchant.dishType }}</span>
                    <span v-if="hasDefaultAddress">&nbsp;&nbsp;{{ merchant.distanceFromDefaultAddress }}km</span>
                    <span>&nbsp;&nbsp;<button @click="enterDishes(merchant.merchantId)">></button></span>
                    <span>&nbsp;<button @click="addToFavorite(merchant.merchantId)">收藏</button></span>
                </li>  
            </ul>  
        </div>  
        <router-view /> <!-- 确保这里可以渲染子路由 -->  
    </div>  
</template>  