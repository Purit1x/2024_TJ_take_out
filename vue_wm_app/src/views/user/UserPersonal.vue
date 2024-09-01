<script setup>
// 导入element-plus的提示组件
import { ElMessage } from 'element-plus';
import { useRouter } from 'vue-router';
import { useStore } from "vuex"  
import { onMounted, ref,watch } from 'vue';
import { userInfo,updateUser,walletRecharge,walletWithdraw,searchFavouriteMerchant,getMerchantsInfo,deleteFavouriteMerchant} from "@/api/user";
const store = useStore();
const router = useRouter();
const refForm =ref(null);
const personalInfo = ref(false);  // 个人信息弹窗状态
const editPI=ref(false)  //编辑个人信息弹窗状态
const isWallet=ref(false);  //是否是钱包界面
const currentUser=ref({})  //编辑用户信息时需要用到
const isRecharge=ref(false);  //充值弹窗状态
const isWithdraw=ref(false); //提现弹窗状态
const isChangeWP=ref(false);  //修改钱包密码弹窗状态
const isFavouriteMerchants=ref(false);  //收藏商户弹窗状态
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
const showPersonalInfo = () => {
    personalInfo.value = true;

}
const leavePersonalInfo = () => {
    personalInfo.value = false;
}
const editPersonalInfo = () => {
    currentUser.value = userForm.value;
    personalInfo.value = false;
    editPI.value = true;
}
const leaveEdit= () => {
    editPI.value = false;
    personalInfo.value = true;
}
const enterWallet=()=>{
    isWallet.value=true;
}
const leaveWallet=()=>{
    isWallet.value=false;
}
const OpenRechargeWindow=()=>{
    currentUser.value = userForm.value;
    isRecharge.value=true;
    isWallet.value=false;
    personalInfo.value = false;
}
const leaveRechargeWindow=()=>{
    isRecharge.value=false;
    isWallet.value=true;
}
const OpenWithdrawWindow=()=>{
    currentUser.value = userForm.value;
    isWithdraw.value=true;
    isWallet.value=false;
    personalInfo.value = false;
}
const leaveWithdrawWindow=()=>{
    isWithdraw.value=false;
    isWallet.value=true;
}
const OpenWPWindow=()=>{
    currentUser.value = userForm.value;
    isChangeWP.value=true;
    isWallet.value=false;
}
const leaveWPWindow=()=>{
    isChangeWP.value=false;
    isWallet.value=true;
}
const enterFavouriteMerchants=()=>{
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
            personalInfo.value = true;
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
    isUserPersonal.value=false;
    router.push('/user-home/personal/myOrder');
}
</script>

<template>
  <div class="content">
    <div v-if="!personalInfo&!editPI&!isWallet&!isRecharge&!isWithdraw&!isChangeWP&!isFavouriteMerchants&isUserPersonal">
        <header>{{userForm.UserName}}的个人中心</header>

        <div><button @click="gobackHome()">返回</button></div>
        <div><button @click="enterFavouriteMerchants()">收藏</button></div>
        <div><button @click="visitingCoupon()">优惠券</button></div>
        <div><button>积分与会员</button></div>
        <div><button @click="visitingMyOrder()">我的订单</button></div>
        <div><button @click="enterWallet()">钱包</button></div>
        <div><button @click="showPersonalInfo()">个人信息</button></div>
        <div><button @click="logout()">退出登录</button></div>
    </div>
    <div v-if="personalInfo&!editPI&!isWallet">
        <div>用户ID: {{userForm.UserId}}</div>
        <div>用户名: {{userForm.UserName}}</div>
        <div>手机号: {{userForm.PhoneNumber}}</div>
        <div>密码: {{userForm.Password}}</div>
        <div>
            <button @click="editPersonalInfo()">编辑</button>
            <button @click="leavePersonalInfo()">返回</button>
        </div>
    </div>
    <div v-if="editPI&!isWallet">
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
    </div>
    <div v-if="isWallet">
        <div>钱包金额：{{userForm.Wallet}}</div>
        <button @click="OpenRechargeWindow">充值</button>
        <button @click="OpenWithdrawWindow">提现</button>
        <button @click="OpenWPWindow">修改支付密码</button>
        <button @click="leaveWallet">返回</button>
    </div>
    <el-form :model="currentUser" :rules="userRules" ref="refForm">
            <div class="recharge" v-if="isRecharge">  <!-- 充值 -->
                <div>充值金额</div>
                <el-form-item label="充值金额" prop="recharge"><input type="number" v-model="currentUser.recharge" placeholder="请输入充值金额" @blur="validateField('recharge')"/></el-form-item>
                <button @click="SaveRecharge">充值</button>
                <button @click="leaveRechargeWindow">返回</button>
            </div>

            <div class="withdraw" v-if="isWithdraw">  <!-- 提现 -->
                <div>提现金额</div>
                <el-form-item label="提现金额" prop="withdrawAmount"><input type="number" v-model="currentUser.withdrawAmount" placeholder="请输入提现金额" @blur="validateField('withdrawAmount')"/></el-form-item>
                <button @click="SaveWithdraw">提现</button>
                <button @click="leaveWithdrawWindow">返回</button>
            </div>
            <div class="changewp" v-if="isChangeWP">  <!-- 修改支付密码 -->
                <div>支付密码</div>
                <el-form-item label="支付密码" prop="WalletPassword"><input type="password" v-model="currentUser.WalletPassword" placeholder="请输入支付密码" @blur="validateField('WalletPassword')"/></el-form-item>
                <div>确认支付密码</div>
                <el-form-item labal="确认支付密码" prop="reWalletPassword"><input type="password" v-model="currentUser.reWalletPassword" placeholder="请再次确认支付密码" @blur="validateField('reWalletPassword')"/></el-form-item>
                <button @click="SaveWalletPassword">修改</button>
                <button @click="leaveWPWindow">返回</button>
            </div>
            <div class="favouritemerchant" v-if="isFavouriteMerchants">  <!-- 收藏商户 -->
                <div>收藏商户:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                    <button @click="leaveFavouriteMerchants()">返回</button>
                </div>
                <ul>
                    <li v-for="item in favouriteMerchantsInfo" :key="item.merchantId">
                        <span>{{item.merchantName}}&nbsp;</span>
                        <span>{{ item.dishType }}&nbsp;</span>
                        <button>></button>
                        <button @click="SavedeleteFM(item.merchantId)">删除收藏</button>
                    </li>
                </ul>
            </div>
    </el-form>
  </div>
    <router-view />
</template>