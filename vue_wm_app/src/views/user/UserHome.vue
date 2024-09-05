<script setup>
import { ref, onMounted, watch, onBeforeUnmount } from 'vue';
import { useRouter } from 'vue-router';
import { useStore } from "vuex";
import { provide } from 'vue';

import { getMerchantIds, getMerchantsInfo, createFavouriteMerchant, GetDefaultAddress, GetUserAddress, getAllMerchantsInfo } from "@/api/user";
import { getDistanceBetweenAddresses, getMerAvgRating, GetMultiSpecialOffer } from "@/api/merchant";

import { ElMessage } from 'element-plus';

const store = useStore();
const router = useRouter();
const user = ref({}); // 初始化用户信息对象  
const isUserHome = ref(true); // 页面是否为用户主页 
const merchantIds = ref([]);  //获取所有商家id
const merchantsInfo = ref([]); // 存储所有商家信息 
const isMenu = ref(false); // 是否在看菜单
const searchQuery = ref(''); // 搜索框内容

const sortField = ref(''); // 搜索框内容
const sortOrder = ref('1'); // 搜索框内容

const filterAddressQuery = ref(''); // 过滤器内容(筛选用)
const filterCouponTypeQuery = ref(false); // 优惠券类型筛选内容(筛选用)
const filterSpecialOfferQuery = ref(false); // 满减筛选内容(筛选用)

const showMerchantsInfo = ref([]); // 显示商家信息列表
const hasDefaultAddress = ref(true); // 用于跟踪是否有默认地址
const DefaultAddress = ref(null); // 用于跟踪默认地址
const DefaultAddressId = ref(null); // 用于跟踪默认地址id
let updateAvgRatingItv = null;
onMounted(async () => {
  const userData = store.state.user;
  if (router.currentRoute.value.path !== '/user-home')
    isUserHome.value = false;
  else
    isUserHome.value = true;
  if (userData) {
    user.value = userData;
  } else {
    router.push('/login');
  }

  getMerchantIds().then(res => {
    merchantIds.value = res.data;
  });
  getAllMerchantsInfo().then(res => {
    merchantsInfo.value = res.data;
    fetchDefaultAddress();
  });
  await fetchMerAvgRating();
  updateAvgRatingItv = setInterval(fetchMerAvgRating, 5000);

});

onBeforeUnmount(() => {
  if (updateAvgRatingItv)
    clearInterval(updateAvgRatingItv);
})


const sortCoupons = () => {
  getAllMerchantsInfo(sortField.value, sortOrder.value).then(res => {
    merchantsInfo.value = res.data;
    fetchDefaultAddress();
  })
}
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
          const distance = await getDistanceBetweenAddresses(merchant.merchantAddress, DefaultAddress.value) / 1000;
          // 将计算的距离添加到商家信息中，单位为米，并保留一位小数  
          merchant.distanceFromDefaultAddress = distance ? (distance * 1000).toFixed(1) : 0;
        }
        // merchantsInfo.value.sort((a, b) => {  //升序排列
        //     const distanceA = parseFloat(a.distanceFromDefaultAddress) || 0;  
        //     const distanceB = parseFloat(b.distanceFromDefaultAddress) || 0;  
        //     return distanceA - distanceB;  
        // });  
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
      newPath !== '/user-home/personal' &&
      (newPath !== '/user-home/cart') &&
      (newPath !== '/user-home/address') &&
      (newPath !== '/user-home/personal/coupon') &&
      (newPath !== '/user-home/personal/coupon/couponPurchase') &&
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
      const specialOfferMatch = !query_specialOffer || (filteredmerchantIds.includes(merchant.merchantId));
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
      <img class="sidebar-img" src="@/assets/my_logo.png" alt="logo" />

      <button class="sidebar-button" @click="gobackHome">
        <img src="@/assets/merchant_home.png" alt="主页" />
        <span class="button-text">主页</span>
      </button>

      <button class="sidebar-button" @click="goToCart">
        <img src="@/assets/user_order.png" alt="购物车" />
        <span class="button-text">购物车</span>
      </button>

      <button class="sidebar-button" @click="goToPersonal">
        <img src="@/assets/merchant_personal.png" alt="个人信息" />
        <span class="button-text">个人信息</span>
      </button>

      <button class="sidebar-button" @click="goToAddress">
        <img src="@/assets/address.png" alt="地址" />
        <span class="button-text">地址</span>
      </button>
      <router-view /> <!-- 渲染子路由 -->
    </div>
  </nav>


  <div class="content">
    <div class="content-header">
      <h1 v-if="isUserHome" class="welcome-text">欢迎，{{ user.userId }}</h1>
    </div>

    <div v-if="isUserHome">
      <div class="search-bar">
        <el-col :span="16">
          <el-input placeholder="搜索店名或类别" v-model="searchQuery" clearable @clear="handleSearch"
            @keydown.enter="handleSearch">
            <template #append>
              <el-button type="primary" @click="handleSearch" class="search-button">
                <el-icon>
                  <search />
                </el-icon>
              </el-button>
            </template>
          </el-input>
        </el-col>
      </div>

      <div class="filter-bar">
        <el-input v-model="filterAddressQuery" placeholder="筛选地址" clearable @keydown.enter="filteredMerchants()"
          class="filter-input"></el-input>
        <el-checkbox v-model="filterCouponTypeQuery" class="filter-checkbox">可使用优惠券</el-checkbox>
        <el-checkbox v-model="filterSpecialOfferQuery" class="filter-checkbox">正在特惠中</el-checkbox>
        <el-button type="primary" @click="filteredMerchants()" class="filter-button">筛选</el-button>
      </div>


      <table class="styled-table">
        <thead>
          <tr>
            <th class="col-name">店名</th>
            <th class="col-type">类别</th>
            <th class="col-distance" v-if="hasDefaultAddress">距离</th>
            <th class="col-Rating">评分</th>
            <th class="col-enter">操作</th>
            <th class="col-favorite">收藏</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="merchant in showMerchantsInfo" :key="merchant.merchantId">
            <td class="col-name">{{ merchant.merchantName }}</td>
            <td class="col-type">{{ merchant.dishType }}</td>
            <td class="col-distance" v-if="hasDefaultAddress">{{ merchant.distanceFromDefaultAddress }}km</td>
            <td class="col-Rating">{{ merchant.avgRating }}</td>
            <td class="col-enter"><button @click="enterDishes(merchant.merchantId)">查看</button></td>
            <td class="col-favorite"><button @click="addToFavorite(merchant.merchantId)">收藏</button></td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<style scoped>
/* 页面背景和整体容器样式 */
body {
  background: linear-gradient(to bottom right, #f4ebf5, #f38bd248);
  margin: 0;
  padding: 0;
  height: 100vh;
  font-family: Arial, sans-serif;
}
</style>

<style scoped lang="scss">
table {
  width: 100%;
  border-collapse: collapse;
}

th,
td {
  border: 1px solid #ddd;
  padding: 8px;
  text-align: center;
}

th {
  background-color: #7BA7AB;
  color: #fff;
  text-align: center;

}

/* 侧边栏样式 */
.sidebar {
  width: 80px;
  /* 调整侧边栏宽度为50px */
  background: linear-gradient(to bottom, #f0d6f2, #f77dd048);
  padding: 20px 0;
  /* 调整为顶部和底部填充 */
  height: 100vh;
  position: fixed;
  top: 0;
  left: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.sidebar-img {
  width: 80%;
  /* 根据新宽度调整logo大小 */
  height: auto;
  margin-bottom: 15px;
}

.sidebar-button {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%;
  margin-bottom: 15px;
  padding: 5px 0;
  background-color: transparent;
  border: none;
  cursor: pointer;
  transition: background-color 0.3s;
  flex-direction: column;
  /* 改为列方向布局，使图标在上，文字在下 */
}

.sidebar-button img {
  width: 30px;
  /* 调整按钮图片大小 */
  height: auto;
}

/* 头部和欢迎信息 */
.content-header {
  margin-bottom: 20px;
  margin-left: 40px;
  /* 调整左边距，避免被侧边栏遮挡 */
}

.welcome-text {
  font-size: 28px;
  font-weight: bold;
  color: #333;
}

/* 搜索栏样式 */
.search-bar {
  display: flex;
  justify-content: flex-start;
  /* 将搜索栏从左侧对齐 */
  margin-left: 40px;
  /* 调整左边距 */
  margin-bottom: 20px;
  width: calc(50% - 60px);
  /* 调整搜索栏宽度 */
}

.search-bar .el-input {
  border-radius: 50px;
  overflow: hidden;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.search-button {
  border-radius: 50%;
  background-color: #DDA0DD;
  border: none;
  color: #fff;
  transition: background-color 0.3s, box-shadow 0.3s;
}

.search-button:hover {
  background-color: #D8BFD8;
  box-shadow: 0 0 8px rgba(255, 105, 180, 0.8);
}

/* 筛选栏样式 */
.filter-bar {
  display: flex;
  align-items: center;
  justify-content: flex-start;
  /* 将筛选栏从左侧对齐 */
  margin-left: 40px;
  /* 调整左边距 */
  margin-bottom: 20px;
  width: 100%;
  /* 使筛选栏宽度占满容器 */
}

.filter-input {
  width: 250px;
  border-radius: 25px;
  background-color: #faf0f0;
  margin-right: 15px;
  /* 给输入框添加右边距 */
}

.filter-checkbox {
  margin-right: 15px;
  /* 给复选框添加右边距 */
  font-size: 16px;
  color: #555;
}

.filter-button {
  padding: 10px 20px;
  border-radius: 25px;
  background-color: #DDA0DD;
  border: none;
  color: #fff;
  cursor: pointer;
  transition: background-color 0.3s, box-shadow 0.3s;
}

.filter-button:hover {
  background-color: #D8BFD8;
  box-shadow: 0 0 8px rgba(255, 105, 180, 0.8);
}

/* 表格样式 */
.styled-table {
  width: calc(100% - 80px);
  /* 调整表格宽度，考虑侧边栏的宽度 */
  border-collapse: collapse;
  max-width: 1300px;
  margin-left: 40px;
  /* 调整表格左边距 */
  margin-right: 40px;
  /* 调整表格右边距 */
  margin-bottom: 20px;
  font-size: 16px;
  box-shadow: 0 0 20px rgba(0, 0, 0, 0.15);
}

.styled-table thead tr {
  background-color: #7BA7AB;
  color: #ffffff;
  text-align: left;
}

.styled-table th,
.styled-table td {
  padding: 12px 15px;
  text-align: center;
}

.styled-table tbody tr {
  border-bottom: 1px solid #dddddd;
}

.styled-table tbody tr:nth-of-type(even) {
  background-color: #f3f3f3;
}

.styled-table tbody tr:last-of-type {
  border-bottom: 2px solid #7BA7AB;
}

.styled-table tbody tr.active-row {
  font-weight: bold;
  color: #7BA7AB;
}


.col-enter button,
.col-favorite button {
  padding: 6px 12px;
  border-radius: 20px;
  background-color: #DDA0DD;
  border: none;
  color: #fff;
  cursor: pointer;
  transition: background-color 0.3s, box-shadow 0.3s;
}

.col-enter button:hover,
.col-favorite button:hover {
  background-color: #D8BFD8;
  box-shadow: 0 0 8px rgba(255, 105, 180, 0.8);
}
</style>
