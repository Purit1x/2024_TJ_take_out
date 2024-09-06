<script setup>
// 导入element-plus的提示组件
import { ElMessage } from 'element-plus';
import { useRouter } from 'vue-router';
import { useStore } from "vuex"  
import { onMounted, ref,watch } from 'vue';
import { userInfo,updateUser,walletRecharge,walletWithdraw,searchFavouriteMerchant,getMerchantsInfo,deleteFavouriteMerchant} from "@/api/user";
import { getQuantityByRegion } from '@/api/platform';
const store = useStore();
const router = useRouter();
const refForm =ref(null);

const editPI=ref(false)  //编辑个人信息弹窗状态
const isFavouriteMerchants=ref(true);  //是否展示收藏商户界面
const isWallet=ref(true);  //是否是钱包界面
const isRecharge=ref(false);  //充值弹窗状态
const isWithdraw=ref(false); //提现弹窗状态
const isChangeWP=ref(false);  //修改钱包密码弹窗状态

const currentUser=ref({})  //编辑用户信息时需要用到
const favouriteMerchantIds=ref([]);  //收藏的商户Id列表
const favouriteMerchantsInfo=ref([]);  //收藏的商户信息列表
const isUserPersonal=ref(true);  //是否是用户主页
const userForm = ref({
    UserId: 0,
    UserName:'',
    PhoneNumber: '',
    Password: '',
    rePassword:'',  
    Wallet: 0,
    WalletPassword: '',
    reWalletPassword:'',
    recharge:0,
    withdrawAmount:0,
});
onMounted(() => {
    const user = store.state.user;
    userInfo(user.userId)
        .then((res) => {
            userForm.value.UserId=res.data.userId;
            userForm.value.UserName=res.data.userName;
            userForm.value.PhoneNumber=res.data.phoneNumber;
            userForm.value.Password=res.data.password;
            userForm.value.Wallet=res.data.wallet;
            userForm.value.WalletPassword=res.data.walletPassword;
        })
        .catch((err) => {
            console.log(err);
        });
    searchFavouriteMerchant(user.userId)
        .then((res) => {
            favouriteMerchantIds.value=res.data;
            return Promise.all(favouriteMerchantIds.value.map(id => getMerchantsInfo(id))); // 并发请求所有商家信息
        }).then(responses => {
            favouriteMerchantsInfo.value = responses.map(response => response.data); // 提取商家信息  
        }).catch(err => {  
            ElMessage.error('获取商家id失败'); 
    }); 
    if(router.currentRoute.value.path !== '/user-home/personal')
        isUserPersonal.value = false;
    else
        isUserPersonal.value = true; 
});
watch(  
    () => router.currentRoute.value.path,  
    (newPath) => {  
        userInfo(userForm.value.UserId)
            .then((res) => {
                userForm.value.Wallet=res.data.wallet;
            })
            .catch((err) => {
                console.log(err);
            })
    }
);  
watch(  
    () => router.currentRoute.value.path,  
    (newPath) => {  
        if (newPath.startsWith('/user-home/personal')&& newPath !== '/user-home/personal/coupon'&&newPath !== '/user-home/personal/coupon/couponPurchase'&&newPath !=='/user-home/personal/myOrder') {  
            isUserPersonal.value = true; // 返回到商家主页时显示欢迎信息和按钮  
        } else {  
            isUserPersonal.value = false; // 进入子路由时隐藏  
        } 
        localStorage.setItem('isUserPersonal', isUserPersonal.value);  
    }  
);  
const checkRePassword = (rule,value,callback) => {
    if(value == ''){
        callback(new Error('请再次确认密码'))
    } else if(value !==currentUser.value.Password){
        callback('二次确认密码不相同请重新输入')
    }else{
        callback()
    }
}
const checkWalletRePassword = (rule,value,callback) => {
    if(value == ''){
        callback(new Error('请再次确认钱包密码'))
    } else if( value !== currentUser.value.WalletPassword){
        callback('二次确认钱包密码不相同请重新输入')
    } else{
        callback()
    }
}
const userRules = ref({  
    UserName: [{ required: true, message: '请输入用户名', trigger: 'blur' }],  
    PhoneNumber: [
        { required: true, message: '请输入电话号码', trigger: 'blur' },
        {  
            pattern: /^[0-9]{11}$/, // 确保输入是长度为11的数字  
            message: '电话号码必须是11位数字', // 错误提示消息  
            trigger: 'blur', 
        },  
    ],
    Password: [{ required: true, message: '请输入密码', trigger: 'blur' }, { min: 5, max: 16, message: '请输入长度5~16非空字符', trigger: 'blur' }], 
    rePassword:[{validator:checkRePassword,trigger:'blur'}], //校验二次输入密码是否相同
    WalletPassword:[
        {required:true, message:'请输入支付密码', trigger:'blur'},
        {  
            pattern: /^[0-9]{6}$/, 
            message: '支付密码必须是6位数字', // 错误提示消息  
            trigger: 'blur', 
        },  
    ],
    reWalletPassword:[{validator:checkWalletRePassword,trigger:'blur'}], //校验二次输入密码是否相同
    recharge:[
        {required:true, message:'请输入充值金额', trigger:'blur'},
        {pattern: /^[0-9]/, 
            message: '充值金额必须是数字', 
            trigger: 'blur', 
        },
    ], 
    withdrawAmount:[
        {required:true, message:'请输入提现金额', trigger:'blur'},
        {pattern: /^[0-9]/, 
            message: '提现金额必须是数字', 
            trigger: 'blur', 
        },
    ], 
});
const validateField = (field) => {  //编辑规则的应用
    refForm.value.validateField(field, (valid) => {  
        if (!valid) {  
            console.log(`验证失败: ${field}`); // 可以根据需要修改  
        }  
    });  
};  
const gobackHome = () => {
    router.push('/user-home');
}
const logout=()=> {
    store.dispatch('clearUser'); 
    router.push('/login') 
}
const setView = () => {
    editPI.value = false;
    isFavouriteMerchants.value=false;
    isWallet.value=false;
    isRecharge.value=false;
    isWithdraw.value=false;
    isChangeWP.value=false;
}
// const showPersonalInfo = () => {
//     personalInfo.value = true;
// }
// const leavePersonalInfo = () => {
//     personalInfo.value = false;
// }
const editPersonalInfo = () => {
    currentUser.value= {...userForm.value};
    // personalInfo.value = false;
    setView();
    editPI.value = true;
}
const leaveEdit= () => {
    editPI.value = false;
    // personalInfo.value = true;
}
const enterWallet=()=>{
    setView();
    isWallet.value=true;
}
const leaveWallet=()=>{
    isWallet.value=false;
}
const OpenRechargeWindow=()=>{
    currentUser.value= {...userForm.value};
    setView();
    isWallet.value=true;
    isRecharge.value=true;

    // isWallet.value=false;
    // personalInfo.value = false;
}
const leaveRechargeWindow=()=>{
    isRecharge.value=false;
    // isWallet.value=true;
}
const OpenWithdrawWindow=()=>{
    currentUser.value= {...userForm.value};  
    setView();
    isWallet.value=true;
    isWithdraw.value=true;
    // isWallet.value=false;
    // personalInfo.value = false;
}
const leaveWithdrawWindow=()=>{
    isWithdraw.value=false;
    // isWallet.value=true;
}
const OpenWPWindow=()=>{
    currentUser.value= {...userForm.value};
    setView();
    isWallet.value=true;
    isChangeWP.value=true;
    // isWallet.value=false;
}
const leaveWPWindow=()=>{
    isChangeWP.value=false;
    // isWallet.value=true;
}
const enterFavouriteMerchants=()=>{
    setView();
    isFavouriteMerchants.value=true;
}
const leaveFavouriteMerchants=()=>{
    isFavouriteMerchants.value=false;
}
const submitEdit = async () => {
    const isValid = await refForm.value.validate();  
    if (!isValid) {  
        return; // 如果不合法，提前退出  
    }  
    updateUser(currentUser.value).then(data=>{
            ElMessage.success('修改成功');
            editPI.value = false;
            // personalInfo.value = true;
            userForm.value = currentUser.value;
        }).catch(error => {
        if (error.response && error.response.data) {  
                const errorCode = error.response.data.errorCode;  
                if (errorCode === 20000) {  
                    ElMessage.error('没有更新数据');  
                } else if (errorCode === 30000) {  
                    ElMessage.error('连接失败' + error.response.data.msg);  
                } else {  
                    ElMessage.error('发生未知错误');  
                }  
            } else {  
                    ElMessage.error('网络错误，请重试');  
            }  
        });     
}
const SaveRecharge=async()=>{
    const isValid = await refForm.value.validate();   
    if (!isValid) return; // 如果不合法，提前退出
    walletRecharge(currentUser.value.UserId,currentUser.value.recharge).then(data=>{
        currentUser.value.Wallet=data.data;
        userForm.value.Wallet=data.data;
        ElMessage.success('充值成功');
        isRecharge.value=false;
        isWallet.value=true;
    }).catch(error => {
        if (error.response && error.response.data) {  
                const errorCode = error.response.data.errorCode;  
                if (errorCode === 20000) {  
                    ElMessage.error('没有更新数据');  
                } else if (errorCode === 30000) {  
                    ElMessage.error('连接失败' + error.response.data.msg);  
                } else {  
                    ElMessage.error('发生未知错误');  
                }  
            } else {  
                    ElMessage.error('网络错误，请重试');  
            }  
    });     
}
const SaveWithdraw=async()=>{
    const isValid = await refForm.value.validate();   
    if (!isValid) return; // 如果不合法，提前退出
    if(currentUser.value.Wallet < currentUser.value.withdrawAmount) 
    {
        ElMessage.error('提现金额超出钱包金额')
        return
    }
    walletWithdraw(currentUser.value.UserId,currentUser.value.withdrawAmount).then(data=>{
        currentUser.value.Wallet=data.data;
        userForm.value.Wallet=data.data;
        ElMessage.success('提现成功');
        isWithdraw.value=false;
        isWallet.value=true;
    }).catch(error => {
        if (error.response && error.response.data) {  
                const errorCode = error.response.data.errorCode;  
                if (errorCode === 20000) {  
                    ElMessage.error('没有更新数据');  
                } else if (errorCode === 30000) {  
                    ElMessage.error('连接失败' + error.response.data.msg);  
                } else {  
                    ElMessage.error('发生未知错误');  
                }  
            } else {  
                    ElMessage.error('网络错误，请重试');  
            }  
    });     
}
const SaveWalletPassword=async()=>{
    const isValid = await refForm.value.validate();   
    if (!isValid) return; // 如果不合法，提前退出
    updateUser(currentUser.value).then(data=>{
        ElMessage.success('修改成功');
        isChangeWP.value=false;
        isWallet.value=true;
    }).catch(error => {
        if (error.response && error.response.data) {  
                const errorCode = error.response.data.errorCode;  
                if (errorCode === 20000) {  
                    ElMessage.error('没有更新数据');  
                } else if (errorCode === 30000) {  
                    ElMessage.error('连接失败' + error.response.data.msg);  
                } else {  
                    ElMessage.error('发生未知错误');  
                }  
            } else {  
                    ElMessage.error('网络错误，请重试');  
            }  
    });     
}
const SavedeleteFM=async(merchantId)=>{
    const userId=userForm.value.UserId;
    const data={
        userId:userId,
        merchantId:merchantId
    };
    deleteFavouriteMerchant(data).then(data=>{
        favouriteMerchantIds.value.splice(favouriteMerchantIds.value.indexOf(merchantId),1);
        favouriteMerchantsInfo.value.splice(favouriteMerchantsInfo.value.findIndex(item=>item.merchantId==merchantId),1);
        ElMessage.success('删除成功');
    }).catch(error => {
        if (error.response && error.response.data) {  
                const errorCode = error.response.data.errorCode;  
                if (errorCode === 20000) {  
                    ElMessage.error('删除未找到');  
                } else if (errorCode === 30000) {  
                    ElMessage.error('连接失败' + error.response.data.msg);  
                } else {  
                    ElMessage.error('发生未知错误');  
                }  
            } else {  
                    ElMessage.error('网络错误，请重试');  
            }  
    });     
    
}
const visitingCoupon=()=>{
    isUserPersonal.value=false;
    router.push('/user-home/personal/coupon');
}
const visitingMyOrder=()=>{
    setView();
    isUserPersonal.value=false;
    router.push('/user-home/personal/myOrder');
}

const enterDishes = (id) => {
  isFavouriteMerchants.value = false;
  router.push('/user-home/merchant/' + id);
};

</script>

<template>
  <div class="content">

    <div v-if="isUserPersonal">
        <header>
            {{userForm.UserName}}的个人中心
            <el-button @click="logout()">退出登录</el-button>
        </header>
        
        <el-descriptions
            class="margin-top"
            title="个人信息"
            :column="2"
            border
            style="margin-bottom: 20px;"
        >
            <template #extra>
            <el-button type="primary" @click="editPersonalInfo()">编辑</el-button>
            </template>
            <el-descriptions-item label-class-name="my-label">
            <template #label>
                <div class="cell-item"><el-icon :style="iconStyle" style="margin-right:5px;"><star /></el-icon> 用户ID</div>
            </template>
            {{userForm.UserId}}
            </el-descriptions-item>
            <el-descriptions-item label-class-name="my-label">
            <template #label>
                <div class="cell-item"><el-icon :style="iconStyle" style="margin-right:5px;"><user /></el-icon> 用户名</div>
            </template>
            {{userForm.UserName}}
            </el-descriptions-item>
            <el-descriptions-item label-class-name="my-label">
            <template #label>
                <div class="cell-item"><el-icon :style="iconStyle" style="margin-right:5px;"><iphone /></el-icon> 联系电话</div>
            </template>
            {{userForm.PhoneNumber}}
            </el-descriptions-item>
            <el-descriptions-item label-class-name="my-label">
            <template #label>
                <div class="cell-item"><el-icon :style="iconStyle" style="margin-right:5px;"><lock /></el-icon> 密码</div>
            </template>
            {{userForm.Password}}
            </el-descriptions-item>
            <el-descriptions-item label-class-name="my-label">
            <template #label>
                <div class="cell-item">我的钱包</div>
            </template>
            <div style="display:flex;justify-content: space-between;align-items:center;">
                ￥{{userForm.Wallet}}
                <div style="text-align: right; gap:20px;">
                    <button @click="OpenRechargeWindow">充值</button>
                    <button @click="OpenWithdrawWindow">提现</button>
                    <button @click="OpenWPWindow">修改支付密码</button>
                </div>
            </div>
            </el-descriptions-item>
        </el-descriptions>
        
        <el-row :gutter="20">
            <!-- <el-col :span="8"><el-button @click="enterFavouriteMerchants()" style="width:100%;">收藏</el-button></el-col> -->
            <el-col :span="12"><el-button @click="visitingCoupon()" style="width:100%;">优惠券</el-button></el-col>
            <el-col :span="12"><el-button @click="visitingMyOrder()" style="width:100%;">我的订单</el-button></el-col>
            <!-- <el-col :span="6"><el-button @click="enterWallet()" style="width:100%;">钱包</el-button></el-col> -->
        </el-row>
    </div>
    
    <!-- 钱包充值弹窗 -->  
    <el-dialog title="充值金额" :model-value="isRecharge" @close="leaveRechargeWindow">  
        <div style="display:flex;flex-direction:row;">
            <el-form :model="currentUser" :rules="userRules" ref="refForm">
                <el-form-item label="充值金额" prop="recharge"><input type="number" v-model="currentUser.recharge" placeholder="请输入充值金额" @blur="validateField('recharge')"/></el-form-item>
            </el-form>
            <el-button @click="SaveRecharge" style="margin-left:30px;">充值</el-button>
        </div>
    </el-dialog>  
    <!-- 钱包提现弹窗 -->  
    <el-dialog title="提现金额" :model-value="isWithdraw" @close="leaveWithdrawWindow">  
        <div style="display:flex;flex-direction:row;">
            <el-form :model="currentUser" :rules="userRules" ref="refForm">
                <el-form-item label="提现金额" prop="withdrawAmount"><input type="number" v-model="currentUser.withdrawAmount" placeholder="请输入提现金额" @blur="validateField('withdrawAmount')"/></el-form-item>
            </el-form>
            <el-button @click="SaveWithdraw" style="margin-left:30px;">提现</el-button>
        </div>
    </el-dialog> 
    <!-- 修改支付密码弹窗 -->  
    <el-dialog title="修改支付密码" :model-value="isChangeWP" @close="leaveWPWindow">  
        <el-form :model="currentUser" :rules="userRules" ref="refForm">
            <el-form-item label="支付密码" prop="WalletPassword"><input type="password" v-model="currentUser.WalletPassword" placeholder="请输入支付密码" @blur="validateField('WalletPassword')"/></el-form-item>
            <el-form-item label="确认支付密码" prop="reWalletPassword"><input type="password" v-model="currentUser.reWalletPassword" placeholder="请再次确认支付密码" @blur="validateField('reWalletPassword')"/></el-form-item>
            <el-button @click="SaveWalletPassword">修改</el-button>
        </el-form>
    </el-dialog> 
    <!-- 编辑个人信息弹窗 -->
    <el-dialog title="修改个人信息" :model-value="editPI" @close="leaveEdit">  
        <el-form ref="refForm" :model="currentUser" :rules="userRules" border>
                <el-form-item label="用户名" prop="UserName"><input v-model="currentUser.UserName" placeholder="用户名" @blur="validateField('UserName')"/></el-form-item>
                <el-form-item label="手机号" prop="PhoneNumber"><input type="number" v-model="currentUser.PhoneNumber" placeholder="手机号" @blur="validateField('PhoneNumber')"/></el-form-item>
                <el-form-item label="密码" prop="Password"><input type="password" v-model="currentUser.Password" placeholder="密码" @blur="validateField('Password')"/></el-form-item>
                <el-form-item label="确认密码" prop="rePassword"><input type="password" v-model="currentUser.rePassword" placeholder="确认密码" @blur="validateField('rePassword')"/></el-form-item>
            </el-form>
            <el-button @click="submitEdit()">提交</el-button>
    </el-dialog> 
    <!-- <div v-if="editPI">
        <div>
            <el-form ref="refForm" :model="currentUser" :rules="userRules">
                <el-form-item label="用户名" prop="UserName"><input v-model="currentUser.UserName" placeholder="用户名" @blur="validateField('UserName')"/></el-form-item>
                <el-form-item label="手机号" prop="PhoneNumber"><input type="number" v-model="currentUser.PhoneNumber" placeholder="手机号" @blur="validateField('PhoneNumber')"/></el-form-item>
                <el-form-item label="密码" prop="Password"><input type="password" v-model="currentUser.Password" placeholder="密码" @blur="validateField('Password')"/></el-form-item>
                <el-form-item label="确认密码" prop="rePassword"><input type="password" v-model="currentUser.rePassword" placeholder="确认密码" @blur="validateField('rePassword')"/></el-form-item>
            </el-form>
            <button @click="submitEdit()">提交</button>
            <button @click="leaveEdit()">取消</button>
        </div>
    </div> -->


    <el-form :model="currentUser" :rules="userRules" ref="refForm">
        <!-- <div class="recharge" v-if="isRecharge">
            <div>充值金额</div>
            <el-form-item label="充值金额" prop="recharge"><input type="number" v-model="currentUser.recharge" placeholder="请输入充值金额" @blur="validateField('recharge')"/></el-form-item>
            <el-button @click="SaveRecharge">充值</el-button>
            <el-button @click="leaveRechargeWindow">返回</el-button>
        </div>

        <div class="withdraw" v-if="isWithdraw">
            <div>提现金额</div>
            <el-form-item label="提现金额" prop="withdrawAmount"><input type="number" v-model="currentUser.withdrawAmount" placeholder="请输入提现金额" @blur="validateField('withdrawAmount')"/></el-form-item>
            <el-button @click="SaveWithdraw">提现</el-button>
            <el-button @click="leaveWithdrawWindow">返回</el-button>
        </div> -->
        <!-- <div class="changewp" v-if="isChangeWP">
            <el-form-item label="支付密码" prop="WalletPassword"><input type="password" v-model="currentUser.WalletPassword" placeholder="请输入支付密码" @blur="validateField('WalletPassword')"/></el-form-item>
            <el-form-item label="确认支付密码" prop="reWalletPassword"><input type="password" v-model="currentUser.reWalletPassword" placeholder="请再次确认支付密码" @blur="validateField('reWalletPassword')"/></el-form-item>
            <el-button @click="SaveWalletPassword">修改</el-button>
            <el-button @click="leaveWPWindow">返回</el-button>
        </div> -->
        <div class="favouritemerchant" v-if="isUserPersonal">  <!-- 收藏商户 -->
            <div style="margin-top:20px;margin-bottom: 20px;font-weight: bold;">收藏商户</div>
            <table class="styled-table">
                <thead>
                    <tr>
                        <th class="col-name">店名</th>
                        <th class="col-type">类别</th>
                        <!-- <th class="col-Rating">评分</th> -->
                        <th class="col-enter">操作</th>
                        <th class="col-favorite">删除收藏</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="item in favouriteMerchantsInfo" :key="item.merchantId">
                        <td class="col-name">{{ item.merchantName }}</td>
                        <td class="col-type">{{ item.dishType }}</td>
                        <!-- <td class="col-Rating">{{ item.avgRating }}</td> -->
                        <td class="col-enter"><button @click="enterDishes(item.merchantId)">查看</button></td>
                        <td class="col-favorite"><button @click="SavedeleteFM(item.merchantId)">删除收藏</button></td>
                    </tr>
                </tbody>
            </table>
        </div>
    </el-form>
  </div>
    <router-view />
</template>

<style scoped>
.content header {
  border-bottom: 1px solid #ccc;
  padding-bottom: 10px;
  height: 50px;
  font-size: 30px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 10px;
}

.el-button {
  margin-right: 5vw;
}

.el-descriptions {
  margin-top: 20px;
}
.cell-item {
  display: flex;
  align-items: center;
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

.el-col-item{
    display: flex;
    justify-content: left;
    align-items: center;
    height: 100%;
    margin-bottom: 20px;
}

.el-button{
  padding: 6px 12px;
  border-radius: 20px;
  background-color: #DDA0DD;
  border: none;
  color: #fff;
  cursor: pointer;
  transition: background-color 0.3s, box-shadow 0.3s;
}
.el-button:hover {
  background-color: #D8BFD8;
  box-shadow: 0 0 8px rgba(255, 105, 180, 0.8);
}

:deep(.my-label) {
  background: #7BA7AB !important;
  color:#ffffff !important;
  width: 16% !important;
}
</style>