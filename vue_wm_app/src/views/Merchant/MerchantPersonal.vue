<script setup>
// 导入element-plus的提示组件
import { ElMessage } from 'element-plus'
import { ref, reactive, onMounted, watch, onBeforeUnmount } from 'vue';
import { useRouter } from 'vue-router';
import { inject } from 'vue';
import store from '@/store';
const router = useRouter();
const merchant = ref({});
import { merchantInfo, getMerPrice,updateMerchant, getMerOrdersWithinThisMonth,getMerOrdersWithinThisDay,walletRecharge,walletWithdraw,assignStationToMerchant,EditMerchantStation,AssignStation} from "@/api/merchant";
import { useStore } from 'vuex';
const isWallet=ref(false);  //是否是钱包界面
const isRecharge=ref(false);  //是否是充值界面
const isWithdraw=ref(false); //是否是提现页面
const isChangeWP=ref(false);  //是否是修改钱包密码界面
const isOnlyShow = ref(true); //是否是修改界面
const refForm =ref(null);
const totalTurnoverWithinThisMonth = ref(0);
const totalTurnoverWithinThisDay = ref(0);
const orderNum = ref(0);
const orderDayNum=ref(0);
const merchantForm = ref({
    MerchantId: 0,
    Password:"",
    MerchantName: "",
    MerchantAddress: "",
    Contact: "",
    CouponType: 0,
    DishType: "",
    rePassword: "",
    TimeforOpenBusiness:"",
    TimeforCloseBusiness:"",
    Wallet:null,
    WalletPassword:"",
    reWalletPassword:"",

});

const currentMerchant=ref({})
let updateTurnoverInterval = null;

const checkRePassword = (rule,value,callback) => {
    if(value == ''){
        callback(new Error('请再次确认密码'))
    } else if(value !==currentMerchant.value.Password){
        callback('二次确认密码不相同请重新输入')
    }else{
        callback()
    }
}
const checkWalletRePassword = (rule,value,callback) => {
    if(value == ''){
        callback(new Error('请再次确认钱包密码'))
    } else if( value !== currentMerchant.value.WalletPassword){
        callback('二次确认钱包密码不相同请重新输入')
    } else{
        callback()
    }
}
const merchantRules = ref({  
    MerchantName: [{ required: true, message: '请输入商家名称', trigger: 'blur' }],  
    MerchantAddress: [{ required: true, message: '请填写商家地址', trigger: 'blur' }],  
    Contact: [
        { required: true, message: '请输入联系方式', trigger: 'blur' },
        {  
            pattern: /^[0-9]{11}$/, // 确保输入是长度为11的数字  
            message: '电话号码必须是11位数字', // 错误提示消息  
            trigger: 'blur', 
        },  
    ],
    DishType: [{ required: true, message: '请输入菜品类型', trigger: 'blur' }], 
    Password: [{ required: true, message: '请输入密码', trigger: 'blur' }, { min: 5, max: 16, message: '请输入长度5~16非空字符', trigger: 'blur' }], 
    rePassword:[{validator:checkRePassword,trigger:'blur'}], //校验二次输入密码是否相同
    TimeforOpenBusiness:[{ required: true, message: '请选择营业开始时间', trigger: 'change' }],
    TimeforCloseBusiness:[{ required: true, message: '请选择营业结束时间', trigger: 'change' }],
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
onMounted(async () => {  
    const merchantData = store.state.merchant;
    merchantInfo(merchantData.MerchantId)
        .then((res) => {
            merchantForm.value.MerchantId = res.data.merchantId;  
            merchantForm.value.MerchantName = res.data.merchantName;  
            merchantForm.value.Password = merchantData.Password;  
            merchantForm.value.rePassword = merchantData.Password;  
            merchantForm.value.MerchantAddress = res.data.merchantAddress;  
            merchantForm.value.Contact = res.data.contact;   
            merchantForm.value.CouponType = res.data.couponType;  
            merchantForm.value.DishType = res.data.dishType;  
            merchantForm.value.TimeforOpenBusiness=res.data.timeforOpenBusiness;  
            merchantForm.value.TimeforCloseBusiness=res.data.timeforCloseBusiness;  
            merchantForm.value.Wallet=res.data.wallet;  
            merchantForm.value.WalletPassword=res.data.walletPassword;  
            
        })
        .catch((err) => {
            console.log(err);
        });
    await renewTurnoverStat();
    updateTurnoverInterval = setInterval(renewTurnoverStat, 10000);

});
const formatTime=(seconds)=> {  
    const hours = Math.floor(seconds / 3600);  
    const minutes = Math.floor((seconds % 3600) / 60);  
    const secs = seconds % 60;  
    return `${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}:${String(secs).padStart(2, '0')}`;  
}  

onBeforeUnmount(() => {
    if (updateTurnoverInterval) {
        clearInterval(updateTurnoverInterval);  // 清除定时器
    }
})

function editMerchant() {
    // 除去验证结果
    currentMerchant.value.MerchantId=merchantForm.value.MerchantId;
    currentMerchant.value.Password=merchantForm.value.Password;
    currentMerchant.value.rePassword=merchantForm.value.rePassword;
    currentMerchant.value.MerchantName=merchantForm.value.MerchantName;
    currentMerchant.value.MerchantAddress= merchantForm.value.MerchantAddress;
    currentMerchant.value.Contact=merchantForm.value.Contact;
    currentMerchant.value.CouponType= merchantForm.value.CouponType;
    currentMerchant.value.DishType= merchantForm.value.DishType;
    currentMerchant.value.TimeforOpenBusiness=merchantForm.value.TimeforOpenBusiness;
    currentMerchant.value.TimeforCloseBusiness=merchantForm.value.TimeforCloseBusiness;
    currentMerchant.value.Wallet=merchantForm.value.Wallet;
    currentMerchant.value.WalletPassword=merchantForm.value.WalletPassword;
    currentMerchant.value.reWalletPassword=merchantForm.value.reWalletPassword;
    currentMerchant.value.recharge=0;
    currentMerchant.value.withdrawAmount=0;
    if(refForm.value)
    {
        refForm.value.clearValidate();
    }

    isOnlyShow.value = false;
}

const saveMerchant = async()=> {
    const isValid = await refForm.value.validate();  
        if (!isValid) {  
            return; // 如果不合法，提前退出  
        }  
    refForm.value.validate((valid) => {
        if (!valid) {
            return;
        }
        const openTime = new Date(currentMerchant.value.TimeforOpenBusiness);  
        const closeTime = new Date(currentMerchant.value.TimeforCloseBusiness);    
        const startHour = openTime.getHours();  
        const startMinute = openTime.getMinutes();  
        const startSecond = openTime.getSeconds();  
        currentMerchant.value.TimeforOpenBusiness = startHour * 3600 + startMinute * 60 + startSecond;  

        const endHour = closeTime.getHours();  
        const endMinute = closeTime.getMinutes();  
        const endSecond = closeTime.getSeconds();  
        currentMerchant.value.TimeforCloseBusiness = endHour * 3600 + endMinute * 60 + endSecond;
        updateMerchant(currentMerchant.value).then(data=>{
            ElMessage.success('修改成功');
            isOnlyShow.value = true;
            merchantForm.value.MerchantName = currentMerchant.value.MerchantName;  
            merchantForm.value.Password = currentMerchant.value.Password;  
            merchantForm.value.rePassword = currentMerchant.value.rePassword;  
            merchantForm.value.MerchantAddress = currentMerchant.value.MerchantAddress;  
            merchantForm.value.Contact = currentMerchant.value.Contact;  
            merchantForm.value.DishType = currentMerchant.value.DishType;  
            merchantForm.value.CouponType = currentMerchant.value.CouponType;  
            merchantForm.value.TimeforOpenBusiness=currentMerchant.value.TimeforOpenBusiness;  
            merchantForm.value.TimeforCloseBusiness=currentMerchant.value.TimeforCloseBusiness;  
            assignStationToMerchant(merchantForm.value.MerchantAddress).then(res=>{
                const stationId=res;
                const data={
                    MerchantId:merchantForm.value.MerchantId,
                    StationId:stationId,
                }
                AssignStation(data).then(data=>{
                    console.log(data);
                }).catch(error => {
                    console.log(error);
                });
                EditMerchantStation(data).then(data=>{
                    console.log(data);
                }).catch(error => {
                    console.log(error);
                });
            }).catch(error => {
                console.log(error);
            });
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
    });
}
const saveWalletPassword= async()=>{
    const isValid = await refForm.value.validate();   
    if (!isValid) return; // 如果不合法，提前退出
    updateMerchant(currentMerchant.value).then(data=>{
        ElMessage.success('修改成功');
        isChangeWP.value=false;
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
    walletRecharge(currentMerchant.value.MerchantId,currentMerchant.value.recharge).then(data=>{
        console.log(data);
        currentMerchant.value.Wallet=data.data;
        merchantForm.value.Wallet=data.data;
        ElMessage.success('充值成功');
        isRecharge.value=false;
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
    if(currentMerchant.value.Wallet < currentMerchant.value.withdrawAmount) 
    {
        ElMessage.error('提现金额超出钱包金额')
        return
    }
    walletWithdraw(currentMerchant.value.MerchantId,currentMerchant.value.withdrawAmount).then(data=>{
        currentMerchant.value.Wallet=data.data;
        merchantForm.value.Wallet=data.data;
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

function cancelEdit() {
    isOnlyShow.value = true;
}

function logout() {
    store.dispatch('clearMerchant'); 
    router.push('/login') 
}
function enterWallet(){
    isWallet.value=true;
}
function leaveWallet(){
    isWallet.value=false;
    isRecharge.value=false;
    isWithdraw.value=false;
    isChangeWP.value=false;
    isOnlyShow.value=true;
}
function OpenRechargeWindow(){  //打开充值界面
    if(isRecharge.value===true){
        leaveRechargeWindow();
        return;
    }
    leaveWithdrawWindow();
    leaveWPWindow();
    editMerchant();
    isRecharge.value=true;
}
function leaveRechargeWindow(){  //关闭充值界面
    isRecharge.value=false;
}
function OpenWithdrawWindow(){  //打开提现界面
    if(isWithdraw.value===true){
        leaveWithdrawWindow();
        return;
    }
    leaveRechargeWindow();
    leaveWPWindow();
    editMerchant();
    isWithdraw.value=true;
}
function leaveWithdrawWindow(){  //关闭提现界面
    isWithdraw.value=false;
    isWallet.value=true;
}
function OpenWPWindow(){  //打开修改支付密码界面
    if(isChangeWP.value==true){
        leaveWPWindow();
        return;
    }
    leaveRechargeWindow();
    leaveWithdrawWindow();
    editMerchant();
    isChangeWP.value=true;
}
function leaveWPWindow(){  //关闭修改支付密码界面
    isChangeWP.value=false;
}
function gobackHome(){
    router.push('/merchant-home');
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
// 跳转到满减活动  
const goToSpecialOffer = () => { 
    router.push('/merchant-home/specialOffer');  
    isMerchantHome.value = false; // 进入满减活动页面时隐藏欢迎信息和按钮  
};  
const renewTurnoverStat=async()=>{
    try{
        const ordersData=await getMerOrdersWithinThisMonth(merchantForm.value.MerchantId);
        const ordersDayData=await getMerOrdersWithinThisDay(merchantForm.value.MerchantId);
        if(ordersData===0){
            orderNum.value=0;
            totalTurnoverWithinThisMonth.value=0;
            return;

        }
        else{
            orderNum.value=ordersData.length;
            totalTurnoverWithinThisMonth.value=0;
            const promise =await ordersData.map(orderItem=>getMerPrice(orderItem));

            const fees=await Promise.all(promise);
            for(const fee of fees){
                totalTurnoverWithinThisMonth.value+=fee;
            }
            
        }

        if(ordersDayData===0)
        {
            orderDayNum.value=0;
            totalTurnoverWithinThisDay.value=0;
            return;
        }
        else{
            orderDayNum.value=ordersDayData.length;
            totalTurnoverWithinThisDay.value=0;
            const promise =await ordersDayData.map(orderItem=>getMerPrice(orderItem));
            const fees=await Promise.all(promise);
            for(const fee of fees){
                totalTurnoverWithinThisDay.value+=fee;
            }
        }
    }
    catch(error){
        throw error;
    }
}
function displayTotalTurnoverWithinThisMonth() {
    return totalTurnoverWithinThisMonth.value || '加载中...';
}
function displayTotalTurnoverWithinThisDay() {
    return totalTurnoverWithinThisDay.value || '加载中...';
}
</script>

<template>
    <div class="content">
        <div class="person_body" v-if="!isWallet">
            <el-descriptions class="margin-top" title="简介" :column="2" v-if="isOnlyShow">
                <template #extra>

                    <el-button type="primary" size="small" @click="editMerchant" class = "edit-button">编辑</el-button>
                    
                </template>
                <el-descriptions-item>
                    <template #label>
                        <i class="el-icon-user"></i>
                        账户ID 
                    </template>
                    {{ merchantForm.MerchantId }}
                </el-descriptions-item>
                <el-descriptions-item>
                    <template #label>
                        <i class="el-icon-user"></i>
                        商家名称
                    </template>
                    {{ merchantForm.MerchantName }}
                </el-descriptions-item>
                <el-descriptions-item>
                    <template #label>
                        <i class="el-icon-s-custom"></i>
                        商家地址
                    </template>
                    {{ merchantForm.MerchantAddress }}
                </el-descriptions-item>
                <el-descriptions-item>
                    <template #label>
                        <i class="el-icon-odometer"></i>
                        联系方式
                    </template>
                    {{ merchantForm.Contact }}
                </el-descriptions-item>
                <el-descriptions-item>
                    <template #label>
                        <i class="el-icon-tickets"></i>
                        菜品类型
                    </template>
                    {{ merchantForm.DishType }}
                </el-descriptions-item>
                <el-descriptions-item>
                    <template #label>
                        <i class="el-icon-judge"></i>
                        是否允许通用优惠券
                    </template>
                    {{ merchantForm.CouponType === 0 ? '是' : '否' }}
                </el-descriptions-item>
                <el-descriptions-item>
                    <template #label>
                        <i class="el-icon-time"></i>
                        开始营业时间
                    </template>
                    {{ formatTime(merchantForm.TimeforOpenBusiness) }} 
                </el-descriptions-item>
                <el-descriptions-item>
                    <template #label>
                        <i class="el-icon-time"></i>
                        结束营业时间
                    </template>
                    {{ formatTime(merchantForm.TimeforCloseBusiness) }} 
                </el-descriptions-item>
            </el-descriptions>

            <el-form :model="currentMerchant" :rules="merchantRules" ref="refForm" label-width="150px" v-else>
                <div class="updateinfo">
                    <el-form-item label="商家名称" prop="MerchantName">
                        <el-input v-model="currentMerchant.MerchantName"></el-input>
                    </el-form-item>
                    <el-form-item label="密码" prop="Password">
                        <el-input :prefix-icon="Lock" type="password" placeholder="请输入密码" v-model="currentMerchant.Password"></el-input>
                    </el-form-item>
                    <el-form-item label="确认密码" prop="rePassword">
                        <el-input :prefix-icon="Lock" type="rePassword" placeholder="请再次确认密码" v-model="currentMerchant.rePassword"></el-input>
                    </el-form-item>
                    <el-form-item label="商家地址" prop="MerchantAddress">
                        <el-input v-model="currentMerchant.MerchantAddress"></el-input>
                    </el-form-item>
                    <el-form-item label="联系方式" prop="Contact">
                        <el-input v-model="currentMerchant.Contact"></el-input>
                    </el-form-item>
                    <el-form-item label="菜品类型" prop="DishType">
                        <el-input v-model="currentMerchant.DishType"></el-input>
                    </el-form-item>
                    <el-form-item label="是否允许通用优惠券" prop="CouponType">
                        <el-radio-group v-model="currentMerchant.CouponType">  
                            <el-radio label=0>是</el-radio>  
                            <el-radio label=1>否</el-radio>  
                        </el-radio-group>
                    </el-form-item>
                    <el-form-item label="营业开始时间" prop="TimeforOpenBusiness">
                        <el-time-picker placeholder="选择日期" v-model="currentMerchant.TimeforOpenBusiness" :picker-options="{ selectableRange: '00:00:00 - 23:59:59' }"></el-time-picker>
                    </el-form-item>
                    <el-form-item label="营业结束时间" prop="TimeforCloseBusiness">
                        <el-time-picker placeholder="选择日期" v-model="currentMerchant.TimeforCloseBusiness" :picker-options="{ selectableRange: '00:00:00 - 23:59:59' }"></el-time-picker>
                    </el-form-item>
                </div>
                <div class="dialog-footer">
                    <el-button @click="cancelEdit" class = "cancel-button">取 消</el-button>
                    <el-button type="primary" @click="saveMerchant" class = "submit-button">提 交</el-button>
                </div>
            </el-form>
            <div v-if="!isWallet&isOnlyShow" class = "el-descriptions">
                <h4>本月销售额</h4>
                <span>您本月订单总量为:{{ orderNum }}</span><span>  您本月的销售额为：{{ displayTotalTurnoverWithinThisMonth() }}</span>
                <br>
                <h4>本日销售额</h4>
                <span>您本日订单总量为:{{ orderNum }}</span><span>  您本日的销售额为：{{ displayTotalTurnoverWithinThisDay() }}</span>
            </div>
            <button v-if="isOnlyShow" @click="enterWallet" class = "wallet-button">钱包</button>
            <br>
            <button v-if="isOnlyShow" @click="logout" class = "exit-button">退出登录</button>
        </div>
        <div class="wallet" v-if="isWallet" >  <!-- 钱包 -->
            <div>钱包金额：{{merchantForm.Wallet}}</div>
            <button @click="OpenRechargeWindow">充值</button>
            <button @click="OpenWithdrawWindow">提现</button>
            <button @click="OpenWPWindow">修改支付密码</button>
            <button @click="leaveWallet">返回</button>
        </div>
        <el-form :model="currentMerchant" :rules="merchantRules" ref="refForm">
            <div class="recharge" v-if="isRecharge">  <!-- 充值 -->
                <div>充值金额</div>
                <el-form-item label="充值金额" prop="recharge"><input type="number" v-model="currentMerchant.recharge" placeholder="请输入充值金额" @blur="validateField('recharge')"/></el-form-item>
                <button @click="SaveRecharge">充值</button>    
                <button @click="leaveRechargeWindow">关闭</button>
            </div>
            <div class="withdraw" v-if="isWithdraw">  <!-- 提现 -->
                <div>提现金额</div>
                <el-form-item label="提现金额" prop="withdrawAmount"><input type="number" v-model="currentMerchant.withdrawAmount" placeholder="请输入提现金额" @blur="validateField('withdrawAmount')"/></el-form-item>
                <button @click="SaveWithdraw">提现</button>
                <button @click="leaveWithdrawWindow">关闭</button>
            </div>
            <div class="changewp" v-if="isChangeWP">  <!-- 修改支付密码 -->
                <div>支付密码</div>
                <el-form-item label="支付密码" prop="WalletPassword"><input type="password" v-model="currentMerchant.WalletPassword" placeholder="请输入支付密码" @blur="validateField('WalletPassword')"/></el-form-item>
                <div>确认支付密码</div>
                <el-form-item labal="确认支付密码" prop="reWalletPassword"><input type="password" v-model="currentMerchant.reWalletPassword" placeholder="请再次确认支付密码" @blur="validateField('reWalletPassword')"/></el-form-item>
                <button @click="saveWalletPassword">修改</button>
                <button @click="leaveWPWindow">关闭</button>
            </div>
        </el-form>
    </div>
    
</template>

<style scoped lang = "scss">

/* Wallet Section */
.wallet {
  background-color: #ffd666;  /* 背景颜色 */
  padding: 20px;              /* 内边距 */
  border-radius: 20px;         /* 圆角 */
  border:2px, solid, black;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);  /* 盒子阴影 */
  margin-bottom: 20px;        /* 下边距 */
  margin-right: 30px;
}

.wallet button {
  background-color: #f59322;  /* 按钮背景颜色 */
  color: white;               /* 按钮字体颜色 */
  padding: 10px 20px;         /* 按钮内边距 */
  margin-right: 10px;         /* 按钮右边距 */
  border: none;               /* 去掉按钮边框 */
  border-radius: 15px;         /* 按钮圆角 */
  cursor: pointer;            /* 鼠标悬停时显示手形光标 */
}

.wallet button:hover {
  background-color: #ba8200;  /* 悬停时的背景颜色 */
}

.wallet div {
  font-size: 18px;            /* 钱包金额字体大小 */
  margin-bottom: 10px;        /* 钱包金额下边距 */
}

/* Recharge Section */
.recharge, .withdraw, .changewp {
  background-color: #fff;     /* 背景颜色 */
  padding: 20px;              /* 内边距 */
  border-radius: 8px;         /* 圆角 */
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);  /* 盒子阴影 */
  margin-bottom: 20px;        /* 下边距 */
  margin-right: 30px;
}

.recharge div, .withdraw div, .changewp div {
  font-size: 18px;            /* 标题字体大小 */
  margin-bottom: 10px;        /* 标题下边距 */
}

.recharge button, .withdraw button, .changewp button {
  background-color: #2196f3;  /* 按钮背景颜色 */
  color: white;               /* 按钮字体颜色 */
  padding: 10px 20px;         /* 按钮内边距 */
  margin-right: 10px;         /* 按钮右边距 */
  border: none;               /* 去掉按钮边框 */
  border-radius: 5px;         /* 按钮圆角 */
  cursor: pointer;            /* 鼠标悬停时显示手形光标 */
}

.recharge button:hover, .withdraw button:hover, .changewp button:hover {
  background-color: #1e88e5;  /* 悬停时的背景颜色 */
}

.el-form-item {
  margin-bottom: 15px;        /* 表单项下边距 */
}

.el-form-item input {
  width: 100%;                /* 输入框宽度 */
  padding: 10px;              /* 输入框内边距 */
  border-radius: 4px;         /* 输入框圆角 */
  border: 1px solid #ccc;     /* 输入框边框 */
  box-sizing: border-box;     /* 盒子大小包含内边距和边框 */
}

/* style */
.person_body {
  padding: 20px;
  background-color: #ffd666;
  border: 2px solid #000000;
  border-radius: 20px;
  margin-right: 40px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
}

.margin-top {
  margin-top: 10px;
}

/* el-descriptions 样式 */
.el-descriptions {
  background-color: #fff;
  border: 2px solid #ffcc00;
  border-radius: 20px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.el-descriptions-item {
  margin-bottom: 10px;
}

.el-descriptions-item i {
  margin-right: 8px;
  color: #409eff;
}

/* 按钮样式 */
.el-button {
  margin-left: 10px;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  padding: 10px 0;
}

/* 编辑表单样式 */
.updateinfo {
  margin-top: 20px;
}

.el-form-item {
  margin-bottom: 15px;
}

.el-input,
.el-time-picker {
  width: 100%;
}

.el-radio-group {
  display: flex;
  align-items: center;
}

.el-radio {
  margin-right: 20px;
}

/* 钱包按钮样式 */
/*button {
  margin-top: 20px;
  padding: 10px 20px;
  font-size: 16px;
  border: none;
  border-radius: 4px;
  background-color: #409eff;
  color: #fff;
  cursor: pointer;
}*/

.wallet-button {
    margin-top: 20px;
    padding: 10px 20px;
    font-size: 16px;
    border: 2px solid black;
    border-radius: 15px;
    margin-bottom: 10px;
    cursor: pointer;
    background-color: #ffcc00;
    color: black;

    &:hover {
        background-color: #ffdd00;
    }
}

.edit-button {
    cursor: pointer;
    background-color: #ffcc00;
    color: white;

    &:hover {
        background-color: #ffdd00;
    }
}

.submit-button {
    cursor: pointer;
    background-color: #4caf50;
    color: white;

    &:hover {
    background-color: #45a049;
    }
}

.exit-button {
    margin-top: 0;
    padding: 10px 20px;
    font-size: 16px;
    border: 2px solid black;
    border-radius: 15px;
    margin-bottom: 10px;
    cursor: pointer;
    background-color: #f44336;
    color: white;

    &:hover {
        background-color: #ff5b58;
    }
}

.cancel-button {
    cursor: pointer;
    background-color: #f44336;
    color: white;

    &:hover {
        background-color: #ff5b58;
    }
}

</style>