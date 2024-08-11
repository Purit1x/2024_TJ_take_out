<script setup>  
import { ref, onMounted, watch } from 'vue';  
import { useCookies } from '@vueuse/integrations/useCookies'  
import { useRouter } from 'vue-router';
import { provide } from 'vue';  
const router = useRouter()
const merchant = ref({MerchantId: ''}); // 初始化商家信息对象  
const cookies = useCookies(); // 使用 useCookies 钩子  
const isMerchantHome = ref(true); // 页面是否为商家主页  
onMounted(() => {  
  // 从 cookie 中读取用户信息  
  const merchantData = cookies.get('merchant');  
  cookies.set('merchant', {});
  isMerchantHome.value = true; // 页面是否为商家主页。这里之所以设置这个是因为子路由会默认渲染父路由的一切渲染内容，所以这里设置一个标识符来控制是否显示父路由内容
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
    }  
);  
// 跳转到菜单  
const goToMenu = () => { 
    router.push('/merchant-home/dish');  
};  

// 跳转到个人信息  
const goToPersonal = () => { 
    router.push('/merchant-home/personal');  
};  
// 提供 merchant 对象 给其它子网页 
provide('merchant', merchant); 
</script>  

<template>  
    <div>  
        <h1 v-if="isMerchantHome">欢迎，{{merchant.MerchantId}}</h1>  
        <!-- 渲染子路由 -->
        <router-view /> 
         <!-- 按钮 -->
        <button v-if="isMerchantHome" @click="goToMenu">菜单</button>  
        <button v-if="isMerchantHome" @click="goToPersonal">我的</button>  
    </div>  
</template>  