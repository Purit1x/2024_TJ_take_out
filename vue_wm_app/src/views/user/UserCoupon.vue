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
    updateCoupons();
});
const updateCoupons=()=>{
    userCoupons.value = [];
    couponPurchaseList.value = [];
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
}
watch(  
    () => router.currentRoute.value.path,  
    (newPath) => {  
        if (newPath.startsWith('/user-home/personal/coupon') && newPath !== '/user-home/personal/coupon/couponPurchase') {  
            isUserCoupon.value = true; // 返回到商家主页时显示欢迎信息和按钮  
            updateCoupons();
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
function formatDateTime(time) { 
    const date = new Date(time); 
    if (isNaN(date.getTime())) { 
        return null; // 或者处理无效日期的逻辑  
    } 
    const year = date.getFullYear(); 
    const month = String(date.getMonth() + 1).padStart(2, '0'); // 月份从0开始  
    const day = String(date.getDate()).padStart(2, '0'); 
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0'); 
    const seconds = String(date.getSeconds()).padStart(2, '0'); 

    return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`; 
}
</script>

<template>
  <div class="content">
    <div v-if="isUserCoupon&&!isShowCouponInfo&&!isPurchaseList&&!isPurchaseInfo">
        <header>我的优惠券
            <el-button @click="gobackHome">返回</el-button>
        </header>
        <el-button @click="enterCouponPurchase">购买优惠券</el-button>&nbsp;&nbsp;
        <el-button @click="enterPurchaseList">购买记录</el-button>&nbsp;&nbsp;

        <div class="search-bar">
            <el-col :span="8">
                <el-input  placeholder="搜索优惠券名称" v-model="searchQuery" clearable @clear="handleSearch" @keydown.enter="handleSearch">
                <template #append>
                <el-button type="primary" @click="handleSearch"><el-icon><search /></el-icon></el-button>
                </template>
                </el-input>
            </el-col>
        </div> 
        <!-- <ul>  
            <li v-for="coupon in showUserCoupons" :key="coupon.couponId">  
                <span>{{ coupon.coupon.couponName }}</span> 
                <span>&nbsp;&nbsp;满{{ coupon.coupon.minPrice }}减{{ coupon.coupon.couponValue }}元</span> 
                <span>&nbsp;&nbsp;有效期至{{ coupon.expirationDate }}</span> 
                <span>&nbsp;&nbsp;&nbsp; × {{ coupon.amountOwned }}</span>
                <span>&nbsp;&nbsp;<button @click="enterCouponInfo(coupon)">></button></span>
            </li>  
        </ul>  -->
        <el-scrollbar max-height="700px">
        <table class="styled-table">
            <thead>
                <tr>
                    <th>券名</th>
                    <th>满减</th>
                    <th>数量</th>
                    <th>有效期</th>
                    <th>类别</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="coupon in showUserCoupons" :key="coupon.couponId">
                    <td>{{ coupon.coupon.couponName }}</td>
                    <td>满{{ coupon.coupon.minPrice }}减{{ coupon.coupon.couponValue }}元</td>
                    <td>{{ coupon.amountOwned }}张</td>
                    <td>{{ formatDateTime(coupon.expirationDate) }}</td>
                    <td><el-tag size="large">{{ coupon.coupon.couponType===0?'通用券':'特殊券' }}</el-tag></td>
                </tr>
            </tbody>
        </table>
        </el-scrollbar>
    </div>
    <!-- <div v-if="isShowCouponInfo">
        <h2>优惠券详情</h2>
        <div>券名：{{ currentUserCoupon.coupon.couponName }}</div>
        <div>满减：满{{ currentUserCoupon.coupon.minPrice }}减{{ currentUserCoupon.coupon.couponValue }}元</div>
        <div>有效期至{{ currentUserCoupon.expirationDate }}</div>
        <div>数量：{{ currentUserCoupon.amountOwned }}张</div>
        <div>类型：{{ currentUserCoupon.coupon.couponType===0?'通用券':'特殊券' }}</div>
        <div><button @click="leaveCouponInfo()">返回</button></div>
    </div> -->
    <div v-if="isPurchaseList&!isPurchaseInfo">
        <header>购买记录
            <el-button @click="leavePurchaseList()">返回</el-button>
        </header>

        <div class="search-bar">
            <el-col :span="8">
                <el-input  placeholder="搜索优惠券名称" v-model="searchPurchaseQuery" clearable @clear="handlePurchaseSearch" @keydown.enter="handlePurchaseSearch">
                <template #append>
                <el-button type="primary" @click="handlePurchaseSearch"><el-icon><search /></el-icon></el-button>
                </template>
                </el-input>
            </el-col>
        </div>
        <!-- <ul>  
            <li v-for="coupon in showCouponPurchaseList" :key="coupon.couponPurchaseId">  
                <span>{{ coupon.coupon.couponName }}</span> 
                <span>&nbsp;&nbsp;满{{ coupon.coupon.minPrice }}减{{ coupon.coupon.couponValue }}元</span> 
                <span>&nbsp;&nbsp;购买时间：{{ coupon.purchasingTimestamp }}</span> 
                <span>&nbsp;&nbsp;&nbsp; × {{ coupon.purchasingAmount }}</span>
                <span>&nbsp;&nbsp;<button @click="enterPurchaseInfo(coupon)">></button></span>
            </li>  
        </ul>  -->
        <div>
            <el-scrollbar max-height="700px">
            <table class="styled-table">
                <thead>
                    <tr>
                        <th>券名</th>
                        <th>满减</th>
                        <th>数量</th>
                        <th>购买时间</th>
                        <th>类别</th>
                        <th>操作</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="coupon in showCouponPurchaseList" :key="coupon.couponPurchaseId">
                        <td>{{ coupon.coupon.couponName }}</td>
                        <td>满{{ coupon.coupon.minPrice }}减{{ coupon.coupon.couponValue }}元</td>
                        <td>{{ coupon.purchasingAmount}}张</td>
                        <td>{{ formatDateTime(coupon.purchasingTimestamp) }}</td>
                        <td><el-tag size="large">{{ coupon.coupon.couponType===0?'通用券':'特殊券' }}</el-tag></td>
                        <td><button @click="enterPurchaseInfo(coupon)">查看</button></td>
                    </tr>
                </tbody>
            </table>
            </el-scrollbar>
        </div>
    </div>
    <div v-if="isPurchaseInfo">
        <!-- <h2>购买详情</h2>
        <div>订单号：{{ currentPurchase.couponPurchaseId }}</div>
        <div>券名：{{ currentPurchase.coupon.couponName }}</div>
        <div>满减：满{{ currentPurchase.coupon.minPrice }}减{{ currentPurchase.coupon.couponValue }}元</div>
        <div>购买时间：{{ currentPurchase.purchasingTimestamp }}</div>
        <div>数量：{{ currentPurchase.purchasingAmount }}张</div>
        <div>类型：{{ currentPurchase.coupon.couponType===0?'通用券':'特殊券' }}</div>
        <div>单价：{{ currentPurchase.coupon.couponPrice }}</div>
        <div>总价：{{ currentPurchase.coupon.couponPrice * currentPurchase.purchasingAmount }}</div>
        <div>有效期：{{ currentPurchase.coupon.periodOfValidity }}天</div>
        <div><button @click="leavePurchaseInfo()">返回</button></div> -->
        <header>购买详情
            <el-button @click="leavePurchaseInfo()">返回</el-button>
        </header>
        <div style="margin-bottom: 20px; margin-top:20px; width: 90%;display: flex;white-space: nowrap;justify-content: center;height: 100%;">
            <el-descriptions
                :column="3"
                size="large"
                border
            >
                <el-descriptions-item label-class-name="my-label">
                <template #label>
                    <div class="cell-item">订单号</div>
                </template>
                {{ currentPurchase.couponPurchaseId }}
                </el-descriptions-item>
                <el-descriptions-item label-class-name="my-label">
                <template #label>
                    <div class="cell-item">券名</div>
                </template>
                {{ currentPurchase.coupon.couponName }}
                </el-descriptions-item>
                <el-descriptions-item label-class-name="my-label">
                <template #label>
                    <div class="cell-item">满减</div>
                </template>
                满{{ currentPurchase.coupon.minPrice }}减{{ currentPurchase.coupon.couponValue }}元
                </el-descriptions-item>
                <el-descriptions-item label-class-name="my-label">
                <template #label>
                    <div class="cell-item">购买时间</div>
                </template>
                {{ formatDateTime(currentPurchase.purchasingTimestamp) }}
                </el-descriptions-item>
                <el-descriptions-item label-class-name="my-label">
                <template #label>
                    <div class="cell-item">数量</div>
                </template>
                {{ currentPurchase.purchasingAmount }}张
                </el-descriptions-item>
                <el-descriptions-item label-class-name="my-label">
                <template #label>
                    <div class="cell-item">类型</div>
                </template>
                <el-tag size="large">{{ currentPurchase.coupon.couponType===0?'通用券':'特殊券' }}</el-tag>
                </el-descriptions-item>
                <el-descriptions-item label-class-name="my-label">
                <template #label>
                    <div class="cell-item">单价</div>
                </template>
                {{ currentPurchase.coupon.couponPrice }}元
                </el-descriptions-item>
                <el-descriptions-item label-class-name="my-label">
                <template #label>
                    <div class="cell-item">总价</div>
                </template>
                {{ currentPurchase.coupon.couponPrice * currentPurchase.purchasingAmount }}元
                </el-descriptions-item>
                <el-descriptions-item label-class-name="my-label">
                <template #label>
                    <div class="cell-item">有效期</div>
                </template>
                {{ currentPurchase.coupon.periodOfValidity }}天
                </el-descriptions-item>
            </el-descriptions>
        </div>
    </div>
  </div>
    <router-view />
</template>

<style scoped>
.search-bar {
    margin-top: 20px;
    margin-bottom: 10px;
}
.el-button{
  padding: 6px 12px;
  border-radius: 20px;
  background-color: #DDA0DD;
  border: none;
  color: #fff;
  cursor: pointer;
  transition: background-color 0.3s, box-shadow 0.3s;
  margin-right: 20px;
}
.el-button:hover {
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

:deep(.my-label) {
  background: #7BA7AB !important;
  color:#ffffff !important;
  width: 16% !important;
}
.el-descriptions-item {
  flex: 1; /* 使每个item均匀分布 */
  min-width: 0; /* 防止内容溢出 */
}
</style>