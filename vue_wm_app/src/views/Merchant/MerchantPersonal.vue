<script setup>
// 导入element-plus的提示组件
import { ElMessage } from 'element-plus'
import { ref, reactive, onMounted, watch } from 'vue';
import { useRouter } from 'vue-router';
import { inject } from 'vue';
import store from '@/store';
const router = useRouter();
const merchant = inject('merchant');
import { merchantInfo, updateMerchant, walletRecharge,assignStationToMerchant,EditMerchantStation,AssignStation} from "@/api/merchant";
const isWallet=ref(false);  //是否是钱包界面
const isRecharge=ref(false);  //是否是充值界面
const isChangeWP=ref(false);  //是否是修改钱包密码界面
const refForm =ref(null);

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
});
const validateField = (field) => {  //编辑规则的应用
    refForm.value.validateField(field, (valid) => {  
        if (!valid) {  
            console.log(`验证失败: ${field}`); // 可以根据需要修改  
        }  
    });  
};  
onMounted(() => {  
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
});
const formatTime=(seconds)=> {  
    const hours = Math.floor(seconds / 3600);  
    const minutes = Math.floor((seconds % 3600) / 60);  
    const secs = seconds % 60;  
    return `${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}:${String(secs).padStart(2, '0')}`;  
}  
const isOnlyShow = ref(true);

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
    isChangeWP.value=false;
    isOnlyShow.value=true;
}
function OpenRechargeWindow(){  //打开充值界面
    editMerchant();
    isRecharge.value=true;
}
function leaveRechargeWindow(){  //关闭充值界面
    isRecharge.value=false;
}
function OpenWPWindow(){  //打开修改支付密码界面
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
</script>

<template>
    <div class="content">
        <div class="person_body" v-if="!isWallet">
            <el-descriptions class="margin-top" title="简介" :column="2" border v-if="isOnlyShow">
                <template #extra>
                    <button @click="gobackHome">返回</button>
                    <el-button type="primary" size="small" @click="editMerchant">编辑</el-button>
                    <el-button type="primary" size="small" @click="logout">退出</el-button>
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
                    <el-button @click="cancelEdit">取 消</el-button>
                    <el-button type="primary" @click="saveMerchant">提 交</el-button>
                </div>
            </el-form>
            <button v-if="isOnlyShow" @click="enterWallet">钱包</button>
        </div>
        <div class="wallet" v-if="isWallet">  <!-- 钱包 -->
            <button @click="leaveWallet">返回</button>
            <div>钱包金额：{{merchantForm.Wallet}}</div>
            <button @click="OpenRechargeWindow">充值</button>
            <button @click="OpenWPWindow">修改支付密码</button>
        </div>
        <el-form :model="currentMerchant" :rules="merchantRules" ref="refForm">
            <div class="recharge" v-if="isRecharge">  <!-- 充值 -->
                <button @click="leaveRechargeWindow">返回</button>
                <div>充值金额</div>
                <el-form-item label="充值金额" prop="recharge"><input type="number" v-model="currentMerchant.recharge" placeholder="请输入充值金额" @blur="validateField('recharge')"/></el-form-item>
                <button @click="SaveRecharge">充值</button>
                <button>提现</button>
            </div>
            <div class="changewp" v-if="isChangeWP">  <!-- 修改支付密码 -->
                <button @click="leaveWPWindow">返回</button>
                <div>支付密码</div>
                <el-form-item label="支付密码" prop="WalletPassword"><input type="password" v-model="currentMerchant.WalletPassword" placeholder="请输入支付密码" @blur="validateField('WalletPassword')"/></el-form-item>
                <div>确认支付密码</div>
                <el-form-item labal="确认支付密码" prop="reWalletPassword"><input type="password" v-model="currentMerchant.reWalletPassword" placeholder="请再次确认支付密码" @blur="validateField('reWalletPassword')"/></el-form-item>
                <button @click="saveWalletPassword">修改</button>
            </div>
        </el-form>
    </div>
</template>

<style scoped>
.me-video-player {
    background-color: transparent;
    width: 100%;
    height: 100%;
    object-fit: fill;
    display: block;
    position: fixed;
    left: 0;
    z-index: 0;
    top: 0;
}

.PersonTop {
    width: 1000px;
    height: 140px;
    padding-top: 20px;
    background-color: white;
    margin-top: 30px;
    position: absolute;
    left: 50%;
    transform: translateX(-50%);
    display: flex;
    border-radius: 5px;
}

.PersonTop_img {
    width: 150px;
    height: 120px;
    background-color: #8c939d;
    margin-right: 24px;
    margin-left: 20px;
    overflow: hidden;
    border-radius: 20px;
}

.PersonTop_img img {
    width: 100%;
    height: 100%;
    border-radius: 20px;
}

.PersonTop_text {
    height: 120px;
    width: 880px;
    display: flex;
}

.user_text {
    width: 60%;
    height: 100%;
    line-height: 30px;
}

.user_name {
    font-weight: bold;
}

.user-v {
    margin-bottom: -5px;
}

.user-v-img {
    width: 15px;
    height: 15px;
}

.user-v-font {
    font-size: 15px;
    color: #00c3ff;
}

.user_qianming {
    font-size: 14px;
    color: #999;
}

.user_num {
    width: 40%;
    height: 100%;
    display: flex;
    align-items: center;
}

.user_num>div {
    text-align: center;
    border-right: 1px dotted #999;
    box-sizing: border-box;
    width: 80px;
    height: 40px;
    line-height: 20px;
}

.num_text {
    color: #999;
}

.num_number {
    font-size: 20px;
    color: #333;
}

.el-menu-item>span {
    font-size: 16px;
    color: #999;
}

/*下面部分样式*/
.person_body {
    width: 85vw;
    position:relative;
    left: 0%;
    border-radius: 5px;
    top: 10%;   
}

.person_body_left {
    width: 27%;
    height: 600px;
    border-radius: 5px;
    margin-right: 3%;
    text-align: center;
}

.person_body_list {
    width: 100%;
    height: 50px;
    margin-top: 25px;
    font-size: 22px;
    border-bottom: 1px solid #f0f0f0;
    background-image: -webkit-linear-gradient(left,
            rgb(42, 134, 141),
            #e9e625dc 20%,
            #3498db 40%,
            #e74c3c 60%,
            #09ff009a 80%,
            rgba(82, 196, 204, 0.281) 100%);
    -webkit-text-fill-color: transparent;
    -webkit-background-clip: text;
    -webkit-background-size: 200% 100%;
    -webkit-animation: masked-animation 4s linear infinite;
}

.el-menu-item {
    margin-top: 22px;
}

.person_body_right {
    width: 70%;
    /* height: 500px; */
    border-radius: 5px;
    background-color: white;
}

.box-card {
    height: 500px;
}

/*ui样式*/
.el-button {
    width: 84px;
}
</style>