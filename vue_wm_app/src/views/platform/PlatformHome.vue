<script setup>  
import { useRouter } from 'vue-router';
import { useStore } from "vuex"  
import { ref,onMounted,watch} from 'vue'
const store = useStore()    
const router = useRouter();
const isPlatformHome = ref(true); // 页面是否为用户主页 
onMounted(() => {
    if(router.currentRoute.value.path !== '/platform-home')
        isPlatformHome.value = false;
    else
        isPlatformHome.value = true; 
});
const handleLogout = () => {
    // 退出登录逻辑
    store.dispatch('clearAdmin'); 
    router.push('/login');
}
const gotoMerchant = () => {
    isPlatformHome.value = false;
    router.push('/platform-home/merchant-manage');
}
const gotoOrder = () => {
    isPlatformHome.value = false;
    router.push('/platform-home/order-manage');
}
const gotoCoupon = () => {
    isPlatformHome.value = false;
    router.push('/platform-home/coupon-manage');
}
const gotoRider = () => {
    isPlatformHome.value = false;
    router.push('/platform-home/rider-manage');
}
const gotoComment = () => {
    isPlatformHome.value = false;
    router.push('/platform-home/comment-manage');
}
const gotoStation = () => {
    isPlatformHome.value = false;
    router.push('/platform-home/station-manage');
}
watch(  
    () => router.currentRoute.value.path,  
    (newPath) => {  
        if (newPath.startsWith('/platform-home')&& newPath !== '/platform-home/order-manage' && newPath !== '/platform-home/merchant-manage' && newPath !== '/platform-home/coupon-manage'&& newPath !== '/platform-home/rider-manage'&& newPath !== '/platform-home/comment-manage'&& newPath !== '/platform-home/station-manage') {  
            isPlatformHome.value = true; // 返回到商家主页时显示欢迎信息和按钮  
        } else {  
            isPlatformHome.value = false; // 进入子路由时隐藏  
        } 
        localStorage.setItem('isPlatformHome', isPlatformHome.value);  
    }  
);  
</script>

<template>
    <div v-if="isPlatformHome">
        这里是平台管理主页
        <button @click="handleLogout">退出登录</button>
    </div>
    <div v-if="isPlatformHome">
        展示销售情况
        <div><button @click="gotoOrder">订单管理</button></div>
        <div><button @click="gotoMerchant">商家管理</button></div>
        <div><button @click="gotoCoupon">优惠券管理</button></div>
        <div><button @click="gotoStation">片区管理</button></div>
        <div><button @click="gotoRider">骑手管理</button></div>
        <div><button @click="gotoComment">评论管理</button></div>
    </div>
    <router-view />
</template>