<!-- 8-14 吴昊泽修改 更新了主页样式 统一了菜单和个人信息界面的导航栏 未解决问题已用#Q#标记（乐） -->

<script setup>  
import { ref, onMounted, watch } from 'vue';  
import { useRouter } from 'vue-router';
import { provide } from 'vue';
import { useStore } from "vuex"  
const store = useStore()    
const router = useRouter()
const merchant = ref({MerchantId: '', MerchantName: ''}); // 初始化商家信息对象  
const isMerchantHome = ref(true); // 页面是否为商家主页 

onMounted(() => {  
  // 从 store 中读取用户信息  
  const merchantData = store.state.merchant;
  console.log(merchantData);
  if(router.currentRoute.value.path !== '/merchant-home')
    isMerchantHome.value = false; // 页面是否为商家主页。这里之所以设置这个是因为子路由会默认渲染父路由的一切渲染内容，所以这里设置一个标识符来控制是否显示父路由内容
  else
    isMerchantHome.value = true; 
  if (merchantData) {  
    merchant.value = merchantData;  
  } else {  
    // 用户未登录，跳转到登录页面  
    router.push('/login');
  }  
});  
// 监视路由变化  
watch(  
    () => router.currentRoute.value.path,  
    (newPath) => {  
        if (newPath.startsWith('/merchant-home') && newPath !== '/merchant-home/dish' && newPath !== '/merchant-home/personal') {  
            isMerchantHome.value = true; // 返回到商家主页时显示欢迎信息和按钮  
        } else {  
            isMerchantHome.value = false; // 进入子路由时隐藏  
        } 
        // 保存状态到 localStorage  
        localStorage.setItem('isMerchantHome', isMerchantHome.value);  
    }  
);  
// 跳转到菜单  
const goBack = () => { 
    router.push('/merchant-home');
};  

// 跳转到菜单  
const goToMenu = () => { 
    router.push('/merchant-home/dish');  
    isMerchantHome.value = false; // 进入菜单页面时隐藏欢迎信息和按钮  
};  

// 跳转到个人信息  
const goToPersonal = () => { 
    router.push('/merchant-home/personal');  
    isMerchantHome.value = false; // 进入个人信息页面时隐藏欢迎信息和按钮  
};  
// 提供 merchant 对象 给其它子网页 
provide('merchant', merchant); 
provide('isMerchantHome', isMerchantHome); 
</script>  

<template>
  <div>
    <!-- 左侧导航栏 在dish和personal界面下出现主页按钮的虚影，不知道是哪里的bug-->
    <nav class="sidebar">
      <div class="sidebar-content">
        <img class="sidebar-img" src="@\assets\logo.png" alt="logo"/>
        
        <button class="sidebar-button" @click="goBack">
          <img src="@\assets\merchant_home.png" alt="主页"/>
          <span>主页</span>
        </button>
        
        <button class="sidebar-button" v-if="isMerchantHome" @click="goToMenu">
          <img src="@\assets\merchant_menu.png" alt="菜单"/>
          <span>本店菜单</span>
        </button>
        
        <button class="sidebar-button" v-if="isMerchantHome" @click="goToPersonal">
          <img src="@\assets\merchant_personal.png" alt="个人信息"/>
          <span>个人信息</span>
        </button>
        <router-view /> <!-- 渲染子路由 -->
      </div>
    </nav>

    <!-- 页面内容区域 #Q# 营收尚不确定是否需要添加，根据后续进度调整-->
    <div  v-if="isMerchantHome" class="content">
      <div class="content-header">
        <!-- #Q# 考虑将MerchantId改为MerchantName -->
        <span class="welcome-text">欢迎，{{ merchant.MerchantId }}!</span>
        <span class="revenue-text">今日营收：{{ todayRevenue }}元    </span>
      </div>
      <!-- 当前订单 实际变量根据平台方订单结构调整 -->
      <div class="orders">
        <h2>当前订单</h2>
        <div class="orders-scroll">
          <div class="order-item" v-for="order in orders" :key="order.id">
            <div>交易ID: {{ order.id }}</div>
            <div>交易金额: {{ order.totalAmount }}元</div>
            <div>下单时间: {{ order.orderTime }}</div>
            <div>制作进度: {{ order.progress }}</div>
            <div>骑手是否已送达: {{ order.delivered ? '是' : '否' }}</div>
            <div>用户评价: {{ order.rating ? order.rating : '暂无评价' }}</div>
         </div>
        </div>
      </div>
      <!-- #Q# 菜单预览 还没做完-->
      <div class="menu-preview">
        <h2>菜单预览</h2>
        <img src="@\assets\merchant_menu.png" alt="菜单"/>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  // #Q# 以下内容为网页排版中的测试用例，实际场景需要调用平台方订单数据动态获取
  data() {
      return {
          username: '用户名', // 用户名
          orders: [
              { id: 1, totalAmount: 100, orderTime: '2023-10-01 10:00', progress: '制作中', delivered: false, rating: null },
              { id: 2, totalAmount: 150, orderTime: '2023-10-01 11:00', progress: '已完成', delivered: true, rating: '好评' },
              { id: 3, totalAmount: 150, orderTime: '2023-10-01 11:00', progress: '已完成', delivered: true, rating: '好评' },
              { id: 4, totalAmount: 150, orderTime: '2023-10-01 11:00', progress: '已完成', delivered: true, rating: '好评' },
              { id: 5, totalAmount: 150, orderTime: '2023-10-01 11:00', progress: '已完成', delivered: true, rating: '好评' },
              { id: 6, totalAmount: 150, orderTime: '2023-10-01 11:00', progress: '已完成', delivered: true, rating: '好评' },
          ], // 订单数据
          todayRevenue: 250 // 今日营收数据
      };
  },
};
</script>

<style>
.content {
  padding: 0px;
  margin-left: 100px;
  width: 90vw;
}

.orders{
  margin-bottom: 30px;
}

.orders-scroll {
  max-height: 600px; /* 设置订单区域的最大高度 */
  display: flex;
  flex-direction: column;
  border:1px solid #ccc;
  overflow-y: auto; /* 使订单区域可以滚动 */
  margin-left: 20px;
}

.order-item {
    padding: 10px 0;
    border: 1px solid #ccc;
    display: flex;
    flex-wrap: wrap;
  }

.order-item div{
  margin-left: 20px;
  font-size: 16px;
  flex:1 1 50%;
  box-sizing: border-box;
}

.content-header {
  margin-top: 15px;
  border-bottom: 1px solid #ccc;
  padding-bottom: 10px;
  height: 50px;
  font-size: 30px;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.welcome-text {
  font-size: 35px;
  margin-left: 15px;
}

.revenue-text {
  font-size: 30px;
  color: #666;
  margin-right: 40px;
  font-weight: bold;
}

.menu-preview { 
  margin-bottom: 20px;
}
</style>