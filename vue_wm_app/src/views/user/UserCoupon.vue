<script setup>
import { useStore } from "vuex";
import { ElMessage } from "element-plus";
import { ref, onMounted, watch } from 'vue';  
import { useRouter } from 'vue-router';
import {getUserCoupons,GetCouponInfo,getAllCouponPurchasesByUser} from '@/api/user';
const store = useStore();
const router = useRouter();
const isUserCoupon = ref(true);
const userId=ref('');
const userCoupons=ref([]);  //用户拥有的优惠券列表
const showUserCoupons = ref(false);  //显示的优惠券列表
const searchQuery = ref('');  //搜索条件
const currentUserCoupon = ref({});  //当前选择的优惠券
const isShowCouponInfo = ref(false);  //显示优惠券详情
const isPurchaseList = ref(false);  //显示购买记录
const couponPurchaseList = ref([]);  //购买记录列表
const showCouponPurchaseList = ref([]);  //显示的购买记录列表
const searchPurchaseQuery = ref('');  //搜索购买记录条件
const isPurchaseInfo = ref(false);  //显示购买详情
const currentPurchase = ref({});  //当前选择的购买记录
onMounted(() => {
    userId.value = store.state.user.userId;
    if(router.currentRoute.value.path !== '/user-home/personal/coupon')
        isUserCoupon.value = false;
    else
        isUserCoupon.value = true; 
    getUserCoupons(userId.value).then(data => {
        if (data.data && data.data.length > 0) {  
            for (const coupon of data.data) {  
                GetCouponInfo(coupon.couponId).then(response => {
                    if (response && response.data) {
                        userCoupons.value.push({ ...coupon, coupon: response.data });
                        showUserCoupons.value=userCoupons.value;
                        sortCoupons();
                    }
                });
            }
            console.log(userCoupons.value);
        }
    }).catch(err => {
        ElMessage.error('尚未拥有优惠券');
    });
    getAllCouponPurchasesByUser(userId.value).then(data => {
        if (data.data && data.data.length > 0) {
            for (const coupon of data.data) { 
                GetCouponInfo(coupon.couponId).then(response => {
                    if (response && response.data) {
                        couponPurchaseList.value.push({ ...coupon, coupon: response.data });
                        showCouponPurchaseList.value=couponPurchaseList.value;
                        sortPurchaseList();
                    }
                });
            }
            console.log(couponPurchaseList.value);
        }
    }).catch(err => {
        ElMessage.error('尚未购买优惠券');
    });
});
watch(  
    () => router.currentRoute.value.path,  
    (newPath) => {  
        if (newPath.startsWith('/user-home/personal/coupon') && newPath !== '/user-home/personal/coupon/couponPurchase') {  
            isUserCoupon.value = true; // 返回到商家主页时显示欢迎信息和按钮  
        } else {  
            isUserCoupon.value = false; // 进入子路由时隐藏  
        } 
        localStorage.setItem('isUserCoupon', isUserCoupon.value);  
    }  
);  
// 监视 userCoupons 的变化，并进行排序  
watch(userCoupons, (newCoupons) => {  
    showUserCoupons.value = [...newCoupons]; // 更新显示的优惠券  
    sortCoupons(); // 排序  
});  
const sortCoupons = () => {  //排序优惠券列表
    showUserCoupons.value.sort((a, b) => {  
        return new Date(a.expirationDate) - new Date(b.expirationDate);  
    });  
}
const sortPurchaseList = () => {  //排序购买记录列表
    showCouponPurchaseList.value.sort((a, b) => {  
        return new Date(a.purchaseDate) - new Date(b.purchaseDate);  
    });  
}
const handleSearch = () => {  
    showUserCoupons.value = userCoupons.value.filter(coupon => coupon.coupon.couponName.includes(searchQuery.value));  
    sortCoupons();
}  
const handlePurchaseSearch = () => {  
    showCouponPurchaseList.value = couponPurchaseList.value.filter(purchase => purchase.coupon.couponName.includes(searchPurchaseQuery.value));  
    sortPurchaseList();
}
const enterCouponInfo = (coupon) => {  
    currentUserCoupon.value = coupon;  
    isShowCouponInfo.value = true;
}
const leaveCouponInfo = () => {  
    isShowCouponInfo.value = false;  
    currentUserCoupon.value = {};
}
const gobackHome = () => {
    isUserCoupon.value = false;
    router.push('/user-home/personal');
}
const enterCouponPurchase = () => {
    isUserCoupon.value = false;
    router.push('/user-home/personal/coupon/couponPurchase');
}
const leavePurchaseList = () => {
    isPurchaseList.value = false;
}
const enterPurchaseList = () => {
    isPurchaseList.value = true;
}
const enterPurchaseInfo = (purchase) => {
    currentPurchase.value = purchase;
    isPurchaseInfo.value = true;
}
const leavePurchaseInfo = () => {
    isPurchaseInfo.value = false;
    currentPurchase.value = {};
}
</script>

<template>
    <div v-if="isUserCoupon&&!isShowCouponInfo&&!isPurchaseList&&!isPurchaseInfo">
        <h2>我的优惠券
            <button @click="enterCouponPurchase">购买优惠券</button>&nbsp;&nbsp;
            <button @click="enterPurchaseList">购买记录</button>&nbsp;&nbsp;
            <button @click="gobackHome">返回</button>
        </h2>
        <div>
            <input type="text" v-model="searchQuery" placeholder="搜索优惠券名称" v-on:keyup.enter="handleSearch()"/> 
            <button @click="handleSearch()">搜索</button>
        </div>
        <ul>  
            <li v-for="coupon in showUserCoupons" :key="coupon.couponId">  
                <span>{{ coupon.coupon.couponName }}</span> 
                <span>&nbsp;&nbsp;满{{ coupon.coupon.minPrice }}减{{ coupon.coupon.couponValue }}元</span> 
                <span>&nbsp;&nbsp;有效期至{{ coupon.expirationDate }}</span> 
                <span>&nbsp;&nbsp;&nbsp; × {{ coupon.amountOwned }}</span>
                <span>&nbsp;&nbsp;<button @click="enterCouponInfo(coupon)">></button></span>
            </li>  
        </ul> 
    </div>
    <div v-if="isShowCouponInfo">
        <h2>优惠券详情</h2>
        <div>券名：{{ currentUserCoupon.coupon.couponName }}</div>
        <div>满减：满{{ currentUserCoupon.coupon.minPrice }}减{{ currentUserCoupon.coupon.couponValue }}元</div>
        <div>有效期至{{ currentUserCoupon.expirationDate }}</div>
        <div>数量：{{ currentUserCoupon.amountOwned }}张</div>
        <div>类型：{{ currentUserCoupon.coupon.couponType===0?'通用券':'特殊券' }}</div>
        <div><button @click="leaveCouponInfo()">返回</button></div>
    </div>
    <div v-if="isPurchaseList&&!isPurchaseInfo">
        <h2>购买记录
            <button @click="leavePurchaseList()">返回</button>
        </h2>
        <div>
            <input type="text" v-model="searchPurchaseQuery" placeholder="搜索优惠券名称" v-on:keyup.enter="handlePurchaseSearch()"/> 
            <button @click="handlePurchaseSearch()">搜索</button>
        </div>
        <ul>  
            <li v-for="coupon in showCouponPurchaseList" :key="coupon.couponPurchaseId">  
                <span>{{ coupon.coupon.couponName }}</span> 
                <span>&nbsp;&nbsp;满{{ coupon.coupon.minPrice }}减{{ coupon.coupon.couponValue }}元</span> 
                <span>&nbsp;&nbsp;购买时间：{{ coupon.purchasingTimestamp }}</span> 
                <span>&nbsp;&nbsp;&nbsp; × {{ coupon.purchasingAmount }}</span>
                <span>&nbsp;&nbsp;<button @click="enterPurchaseInfo(coupon)">></button></span>
            </li>  
        </ul> 
    </div>
    <div v-if="isPurchaseInfo">
        <h2>购买详情</h2>
        <div>订单号：{{ currentPurchase.couponPurchaseId }}</div>
        <div>券名：{{ currentPurchase.coupon.couponName }}</div>
        <div>满减：满{{ currentPurchase.coupon.minPrice }}减{{ currentPurchase.coupon.couponValue }}元</div>
        <div>购买时间：{{ currentPurchase.purchasingTimestamp }}</div>
        <div>数量：{{ currentPurchase.purchasingAmount }}张</div>
        <div>类型：{{ currentPurchase.coupon.couponType===0?'通用券':'特殊券' }}</div>
        <div>单价：{{ currentPurchase.coupon.couponPrice }}</div>
        <div>总价：{{ currentPurchase.coupon.couponPrice * currentPurchase.purchasingAmount }}</div>
        <div>有效期：{{ currentPurchase.coupon.periodOfValidity }}天</div>
        <div><button @click="leavePurchaseInfo()">返回</button></div>

    </div>
    
    <router-view />
</template>