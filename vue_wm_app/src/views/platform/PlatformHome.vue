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
const gotoStatistic = () => {
    isPlatformHome.value = false;
    console.log('isPlatformHome:',isPlatformHome.value);
    router.push('/platform-home/statistic-manage');
}
watch(  
    () => router.currentRoute.value.path,  
    (newPath) => {  
        if (newPath.startsWith('/platform-home')&& newPath !== '/platform-home/order-manage' && newPath !== '/platform-home/merchant-manage' && newPath !== '/platform-home/coupon-manage'&& newPath !== '/platform-home/rider-manage'&& newPath !== '/platform-home/comment-manage'&& newPath !== '/platform-home/station-manage' && newPath !== '/platform-home/statistic-manage') {  
            isPlatformHome.value = true; // 返回到商家主页时显示欢迎信息和按钮  
        } else {  
            isPlatformHome.value = false; // 进入子路由时隐藏  
        } 
        localStorage.setItem('isPlatformHome', isPlatformHome.value);  
    }  
);  
</script>

<template>

    <div class="managehome" v-if="isPlatformHome">    
        <div class="headline" v-if="isPlatformHome">
        平台管理主页
        </div>
        <div class="quarter-div"><a  @click="gotoOrder">订单管理</a></div>
        <div class="quarter-div"><a  @click="gotoMerchant">商家管理</a></div>
        <div class="quarter-div"><a  @click="gotoCoupon">优惠券管理</a></div>
        <div class="quarter-div"><a  @click="gotoStation">片区管理</a></div>
        <div class="quarter-div"><a  @click="gotoRider">骑手管理</a></div>
        <div class="quarter-div"><a  @click="gotoComment">评论管理</a></div>       
        <div class="quarter-div"><a  @click="gotoStatistic">数据统计</a></div>   
        <div class="quarter-div"><a  @click="handleLogout">退出登录</a></div>
    </div>
    <router-view />
</template>

<style scoped>
*{
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}

.managehome {
    height: 100%;
    width: 100%;
    position: absolute;
}

.headline {
    height: 10%;
    display: flex;
    justify-content: center;
    align-items: center;
    font-size: 30px;
}


.quarter-div {
    width: calc(100% / 3);
    height: calc(90% / 3);
    float: left;
    display: flex;
    justify-content: center;
    align-items: center;
}

a {
    font-size: 20px;
    /* width: 150px;
    height: 150px; */
    color: #111;
    text-decoration: none;
    position: relative;
    padding: 10px 30px;
    margin: 45px 0;
    overflow: hidden;
    transition: 0.5s;
}

a:hover{
    background-color: #111;
    color: #fff;
    /* box-shadow: 0 0 50px #21ebff; */
    transition-delay: 0.5s;
}

a::before{
    content:'';
    position: absolute;
    top:0;
    left:0;
    width:20px;
    height:20px;
    border-left: 2px solid #111;
    border-top: 2px solid #111;
    transition: 0.5s;
}

a::after{
    content:'';
    position: absolute;
    bottom:0;
    right:0;
    width:20px;
    height:20px;
    border-bottom: 2px solid #111;
    border-right: 2px solid #111;
    transition: 0.5s;

}

a:hover::before, a:hover::after {
    width: 100%;
    height: 100%;
}

</style>