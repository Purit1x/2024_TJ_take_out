<script setup>  
import { ref, onMounted,watch} from 'vue';  
import { useRouter } from 'vue-router';
import { inject } from 'vue'; 
import store from '@/store';
const router = useRouter();
const merchant =inject('merchant');


 
onMounted(() => {  
    merchant.value=store.state.merchant;
});  

const goBack = () => {
    router.push('/merchant-home');
    isMerchantHome.value = true;  
}

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

</script>

<template>  
  <div>  
    <!-- 左侧导航栏 -->
    <nav class="sidebar">
      <div class="sidebar-content">
        <img class="sidebar-img" src="@\assets\logo.png" alt="logo"/>
        <button class="sidebar-button" @click="goBack">
          <img src="@\assets\merchant_home.png" alt="主页"/>
          <span>主页</span>
        </button>

        <button class="sidebar-button" @click="goToMenu">
          <img src="@\assets\merchant_menu.png" alt="菜单"/>
          <span>本店菜单</span>
        </button>
        
        <button class="sidebar-button" @click="goToPersonal">
          <img src="@\assets\merchant_personal.png" alt="个人信息"/>
          <span>个人信息</span>
        </button>
        <router-view /> <!-- 渲染子路由 -->
      </div>
    </nav>
    <div class="content">
        <h1>这里是个人主页页面，{{merchant.MerchantId}}</h1>  
        <!-- 其他用户信息 --> 
    </div>
  </div>  
</template>  