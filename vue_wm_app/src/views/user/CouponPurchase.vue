<script setup>
import { useStore } from "vuex";
import { ElMessage, ElDialog, ElInput } from "element-plus";
import { ref, onMounted, watch } from 'vue';  
import { useRouter } from 'vue-router';
import { getAvailableCoupons ,CreateCouponPurchase ,userInfo} from '@/api/user';
const store = useStore();
const router = useRouter();
const userId=ref('');
const couponList = ref([]);  //可以购买的优惠券列表
const searchQuery = ref('');  //搜索框输入内容
const showCouponsList = ref([]);  //显示的优惠券列表
const sortField = ref('minPrice'); // 默认排序字段  
const sortOrder = ref('asc'); // 默认排序方向，'asc' 表示升序，'desc' 表示降序 
const filterType = ref('all'); // 默认筛选类型为“全部”
const isCouponInfo=ref(false); // 是否显示优惠券详情
const currentCoupon = ref({}); // 当前选择的优惠券
const isPurchasing=ref(false);   // 是否正在购买
const countOfCoupon = ref(1); // 购买数量
const isDialogVisible = ref(false); // 控制弹窗显示  
const paymentPassword = ref(''); // 存储用户输入的支付密码  
const paymentError = ref(''); // 支付密码错误提示  
const correctPassword = ref(false); // 正确的支付密码
onMounted(() => {
    userId.value = store.state.user.userId;
    getAvailableCoupons().then(res => {
        couponList.value = res.data;
        showCouponsList.value = res.data;
        sortCoupons();
    }).catch(err => {
        ElMessage.error('获取优惠券列表失败');
    });
    userInfo(userId.value).then(res => {
        correctPassword.value = res.data.walletPassword;
    }).catch(err => {
        ElMessage.error('获取用户信息失败');
    });
});
const sortCoupons = () => {  
    const sortedList = [...showCouponsList.value]; // 复制原始列表以免影响原数据  
    sortedList.sort((a, b) => {  
        const aValue = a[sortField.value];  
        const bValue = b[sortField.value];  

        // 根据排序方向比较  
        if (sortOrder.value === 'asc') {  
            return aValue < bValue ? -1 : aValue > bValue ? 1 : 0;  
        } else {  
            return aValue > bValue ? -1 : aValue < bValue ? 1 : 0;  
        }  
    });  
    showCouponsList.value = sortedList; // 更新显示的列表  
};  
const filteredCoupons = () => {  
    if (filterType.value === 'all') {  
        return showCouponsList.value; // 返回所有优惠券  
    }  
    return showCouponsList.value.filter(coupon => {  
        return filterType.value === 'general'   
            ? coupon.couponType === 0 // 通用券  
            : coupon.couponType === 1; // 特殊券  
    });  
};  
// 确认购买  
const confirmPurchase = () => {  
    if (!paymentPassword.value) {  
        paymentError.value = '支付密码不能为空'; // 提示用户输入密码  
        return;  
    }  
    if (paymentPassword.value!== correctPassword.value) {  
        paymentError.value = '支付密码错误'; // 提示用户输入错误的密码  
        return;  
    }  
    const purchaseData = {  
        CouponPurchaseId:0,
        PurchasingTimestamp:new Date().toLocaleString(),
        CouponId:currentCoupon.value.couponId,
        UserId:userId.value,
        PurchasingAmount:countOfCoupon.value,
    };  
    console.log(purchaseData);
    CreateCouponPurchase(purchaseData).then(res => {  
        ElMessage.success('购买成功');  
        isDialogVisible.value = false;  
        countOfCoupon.value = 1;  
        paymentPassword.value = '';  
        paymentError.value = '';  
    }).catch(error => {  
        if (error.response && error.response.data) {  
                    const errorCode = error.response.data.errorCode;  

                    if (errorCode === 10000) {  
                        ElMessage.error('优惠券已下架');  
                    } else if (errorCode === 20000) {  
                        ElMessage.error('余额不足');  
                    } else {  
                        ElMessage.error('购买失败，请稍后再试');  
                    }  
                } else {  
                     ElMessage.error('网络错误，请重试');  
                }  
    });  
};  
const submitPurchase = () => {
    isDialogVisible.value = true;  
    paymentPassword.value = ''; // 清空之前输入的密码  
    paymentError.value = ''; // 清空之前的错误提示  
}
const gobackHome = () => {
    router.push('/user-home/personal/coupon');
}
const enterCouponInfo = (coupon) => {
    currentCoupon.value = coupon;
    isCouponInfo.value = true;
}
const leaveCouponInfo = () => {
    isCouponInfo.value = false;
    isPurchasing.value = false;
    currentCoupon.value = {};
}
const addCount = () => {
    countOfCoupon.value++;
}
const subCount = () => {
    if (countOfCoupon.value > 1) {
        countOfCoupon.value--;
    }
}
const cancelPurchase = () => {
    isDialogVisible.value = false;
    isPurchasing.value = false;
    paymentPassword.value = '';
    paymentError.value = '';
}
</script>

<template>
    <div class="content">
    <div v-if="!isCouponInfo">
        <header>购买优惠券
            <el-button @click="gobackHome()">返回</el-button>
        </header>

        <!-- <div>
            <input type="text" v-model="searchQuery" placeholder="搜索优惠券名称" v-on:keyup.enter="handleSearch()"/> 
            <button @click="handleSearch()">搜索</button>
        </div> -->

        <div class="buy-search-bar">
            <el-col :span="8">
                <el-input  placeholder="搜索优惠券名称" v-model="searchQuery" clearable @clear="handleSearch()" @keydown.enter="handleSearch()">
                <template #append>
                <el-button type="primary" @click="handleSearch()"><el-icon><search /></el-icon></el-button>
                </template>
                </el-input>
            </el-col>
        </div>


    <div>  
        <label>排序字段</label>  
        <el-select v-model="sortField" @change="sortCoupons" style="width:120px;margin-left:10px;">  
            <el-option value="minPrice" label="使用额度"></el-option>  
            <el-option value="couponValue" label="券值"></el-option>  
            <el-option value="couponPrice" label="价格"></el-option>  
            <el-option value="quantitySold" label="销量"></el-option>
        </el-select>  
        &nbsp;&nbsp;
        <label>排序方式</label>  
        <el-select v-model="sortOrder" @change="sortCoupons" style="width:120px;margin-left:10px;">  
            <el-option value="asc" label="升序"></el-option>  
            <el-option value="desc" label="降序"></el-option>  
        </el-select>  
        &nbsp;&nbsp;
        <label>筛选类别</label>  
        <el-select v-model="filterType" @change="sortCoupons" style="width:120px;margin-left:10px;">  
            <el-option value="all" label="全部">全部</el-option>  
            <el-option value="general" label="通用券"></el-option>  
            <el-option value="special" label="特殊券"></el-option>  
        </el-select>  
    </div>  
        <!-- <ul>  
            <li v-for="coupon in filteredCoupons()" :key="coupon.couponId">  
                <span>{{ coupon.couponName }}</span> 
                <span>&nbsp;&nbsp;满{{ coupon.minPrice }}减{{ coupon.couponValue }}元</span> 
                <span>&nbsp;&nbsp;{{ coupon.couponType===0?'通用券':'特殊券' }}</span> 
                <span>&nbsp;&nbsp;{{ coupon.couponPrice }}元/张</span> 
                <span>&nbsp;&nbsp;销量：{{ coupon.quantitySold }}张</span> 
                <span>&nbsp;&nbsp;<button @click="enterCouponInfo(coupon)">></button></span>
            </li>  
        </ul>  -->

        <table class="styled-table" style="margin-top: 20px;">
        <thead>
          <tr>
            <th>券名</th>
            <th>满减</th>
            <th>类别</th>
            <th>价格</th>
            <th>销量</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="coupon in filteredCoupons()" :key="coupon.couponId">
            <td>{{ coupon.couponName }}</td>
            <td>满{{ coupon.minPrice }}减{{ coupon.couponValue }}元</td>
            <td><el-tag size="large">{{ coupon.couponType===0?'通用券':'特殊券' }}</el-tag></td>
            <td>{{ coupon.couponPrice }}元/张</td>
            <td>{{ coupon.quantitySold }}张</td>
            <td><el-button @click="enterCouponInfo(coupon)">查看</el-button></td>
          </tr>
        </tbody>
      </table>
    </div>
    <div v-if="isCouponInfo">
        <header>优惠券详情
            <el-button @click="leaveCouponInfo()">返回</el-button>
        </header>
        <!-- <div>券名：{{ currentCoupon.couponName }}</div>
        <div>价值：满{{ currentCoupon.minPrice }}减{{ currentCoupon.couponValue }}元</div>
        <div>类型：{{ currentCoupon.couponType===0?'通用券':'特殊券' }}</div>
        <div>价格：{{ currentCoupon.couponPrice }}元/张</div>
        <div>销量：{{ currentCoupon.quantitySold }}张</div>
        <div>有效期：{{ currentCoupon.periodOfValidity }}天</div> -->

        <el-descriptions
            column="3"
            size="large"
            border
            style="margin-bottom: 20px; width:80%;"
        >
            <el-descriptions-item label-class-name="my-label">
            <template #label>
                <div class="cell-item">券名</div>
            </template>
            {{ currentCoupon.couponName }}
            </el-descriptions-item>
            <el-descriptions-item label-class-name="my-label">
            <template #label>
                <div class="cell-item">价值</div>
            </template>
            满{{ currentCoupon.minPrice }}减{{ currentCoupon.couponValue }}元
            </el-descriptions-item>
            <el-descriptions-item label-class-name="my-label">
            <template #label>
                <div class="cell-item">类型</div>
            </template>
            <el-tag size="large">{{ currentCoupon.couponType===0?'通用券':'特殊券' }}</el-tag>
            </el-descriptions-item>
            <el-descriptions-item label-class-name="my-label">
            <template #label>
                <div class="cell-item">价格</div>
            </template>
            {{ currentCoupon.couponPrice }}元/张
            </el-descriptions-item>
            <el-descriptions-item label-class-name="my-label">
            <template #label>
                <div class="cell-item">销量</div>
            </template>
            {{ currentCoupon.quantitySold }}张
            </el-descriptions-item>
            <el-descriptions-item label-class-name="my-label">
            <template #label>
                <div class="cell-item">有效期</div>
            </template>
            {{ currentCoupon.periodOfValidity }}天
            </el-descriptions-item>
        </el-descriptions>

        <div>
          <el-button @click="isPurchasing=true" style="margin-bottom: 20px;">购买</el-button>
            <div v-if="isPurchasing">
                数量：
                <el-input-number v-model="countOfCoupon" style="margin-left: 10px; margin-right: 10px;" />
                <!-- <el-button @click="subCount()">-</el-button>
                <input type="text" v-model="countOfCoupon" size="2"/>
                <el-button @click="addCount()">+</el-button> -->
                总价：{{ currentCoupon.couponPrice * countOfCoupon }}元
                <el-button @click="submitPurchase()" style="margin-left: 10px;">提交</el-button>
            </div>
        </div>
        <!-- 弹窗 -->  
        <el-dialog title="输入支付密码" :model-value="isDialogVisible" @close="cancelPurchase">  
            <el-input   
                type="password"   
                v-model="paymentPassword"   
                placeholder="请输入6位支付密码"   
                clearable   
            />  
            <div v-if="paymentError" style="color: red;">{{ paymentError }}</div>  
            <template #footer>  
                <el-button @click="cancelPurchase()">取消</el-button>  
                <el-button type="primary" @click="confirmPurchase()">确认</el-button>  
            </template>  
        </el-dialog>  
    </div>
</div>
</template>

<style scoped>
.buy-search-bar {
    margin-top: 10px;
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