<script setup>  
import { ref, onMounted, watch } from 'vue';  
import { useRouter } from 'vue-router';
import { useStore } from "vuex";
import { provide } from 'vue';
import { getMerchantIds,getMerchantsInfo,createFavouriteMerchant,GetDefaultAddress,GetUserAddress } from "@/api/user";

import {getDistanceBetweenAddresses,getMerAvgRating, GetMultiSpecialOffer} from "@/api/merchant";

import { ElMessage } from 'element-plus';

const store = useStore();    
const router = useRouter();
const user = ref({}); // 初始化用户信息对象  
const isUserHome = ref(true); // 页面是否为用户主页 
const merchantIds = ref([]);  //获取所有商家id
const merchantsInfo = ref([]); // 存储所有商家信息 
const isMenu= ref(false); // 是否在看菜单
const searchQuery = ref(''); // 搜索框内容
const filterAddressQuery = ref(''); // 过滤器内容(筛选用)
const filterCouponTypeQuery = ref(false); // 优惠券类型筛选内容(筛选用)
const filterSpecialOfferQuery = ref(false); // 满减筛选内容(筛选用)
const showMerchantsInfo = ref([]); // 显示商家信息列表
const hasDefaultAddress = ref(true); // 用于跟踪是否有默认地址
const DefaultAddress=ref(null); // 用于跟踪默认地址
const DefaultAddressId=ref(null); // 用于跟踪默认地址id
onMounted(async() => {  
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
  
  await getMerchantIds().then(res => {  // 获取所有商家id
    merchantIds.value = res.data;  
    return Promise.all(merchantIds.value.map(id => getMerchantsInfo(id))); // 并发请求所有商家信息
  }).then(responses => {  
    merchantsInfo.value = responses.map(response => response.data); // 提取商家信息  
    fetchDefaultAddress();  // 获取用户默认地址
    
    showMerchantsInfo.value = merchantsInfo.value; // 显示商家信息列表
  }).catch(err => {  
    ElMessage.error('获取商家id失败'); 
  }); 
  await fetchMerAvgRating();
}); 
const fetchMerAvgRating = async () => {
  try {
    for (let info of showMerchantsInfo.value) {
      const avgRating = await getMerAvgRating(info.merchantId);
      console.log(`Merchant ID: ${info.merchantId}, Avg Rating: ${avgRating}`);
      info.avgRating = avgRating; // 将平均评分添加到商家信息对象中
    }
    console.log("商家信息", showMerchantsInfo.value);
  } catch (error) {
    console.error('Failed to fetch merchant average ratings:', error);
  }
};
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
const filteredMerchants = async () => {
  const query_address = filterAddressQuery.value.trim();
  const query_couponType = filterCouponTypeQuery.value;
  const query_specialOffer = filterSpecialOfferQuery.value;

  const specialOfferRes = await GetMultiSpecialOffer(merchantIds.value);
  const filteredmerchantIds = Array.from(new Set(specialOfferRes.data.map(offer => offer.merchantId)));

  if (query_address || query_couponType || query_specialOffer) {
    showMerchantsInfo.value = merchantsInfo.value.filter(merchant => {
      const addressMatch = !query_address || (merchant.merchantAddress && merchant.merchantAddress.includes(query_address));
      const coupontypeMatch = !query_couponType || (merchant.couponType === 0);
      const specialOfferMatch = !query_specialOffer || (merchant.merchantId in filteredmerchantIds);
      return addressMatch && coupontypeMatch && specialOfferMatch;
    });
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
            <!-- <div class="search-bar">
              <input type="text" v-model="searchQuery" placeholder="搜索店名或类别" v-on:keyup.enter="handleSearch()"/> 
              <button @click="handleSearch()">搜索</button>
            </div> -->

            <div class="search-bar">
              <el-col :span="8">
                  <el-input  placeholder="搜索店名或类别" v-model="searchQuery" clearable @clear="handleSearch" @keydown.enter="handleSearch">
                  <template #append>
                    <el-button type="primary" @click="handleSearch"><el-icon><search /></el-icon></el-button>
                  </template>
                  </el-input>
              </el-col>
            </div>

            <div class="filter-bar">
              <input type="text" v-model="filterAddressQuery" placeholder="筛选地址" v-on:keyup.enter="filteredMerchants()"/>
              <label>
                <input type="checkbox" v-model="filterCouponTypeQuery"> 可使用优惠券
              </label>
              <label>
                <input type="checkbox" v-model="filterSpecialOfferQuery"> 正在特惠中
              </label>
              <button @click="filteredMerchants()">筛选</button>
            </div>

            <table>
              <tbody>
                <tr v-for="merchant in showMerchantsInfo" :key="merchant.merchantId">
                  <td class="col-name">{{ merchant.merchantName }}</td> 
                  <td class="col-type">{{ merchant.dishType }}</td>

                  <span v-if="hasDefaultAddress">&nbsp;&nbsp;{{ merchant.distanceFromDefaultAddress }}km</span>
                  <td class="col-Rating">评分：{{ merchant.avgRating }}</td>

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

<style scoped lang="scss">
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
.col-type{width:30%;}

.col-distance{width:20%;}

.col-Rating{width:10%}
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
}
.sidebar-button img {
  width: 100%;
  height: auto;
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

.search-bar {
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

.filter-bar {
  width: 1000px;
  height: 40px;
  display: flex;
  margin-bottom: 20px;
  margin-top: 5px;

  input{
    width: 20%;
  }
  button{
    width: 8%;
  }
  label{
    width: 15%;
    margin-left: 20px;
    display: flex;
    align-items:center;
    border: 1px solid #7BA7AB;
  }
}

</style>