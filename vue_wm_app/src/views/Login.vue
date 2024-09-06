<script setup>
// 导入element-plus的提示组件
import { ElMessage } from 'element-plus'
// 导入用户图标和锁图标
import { User, Lock, Dish, Wallet } from '@element-plus/icons-vue'
// 导入引用
import { ref } from 'vue'
// 状态管理
import { useStore } from "vuex"
const store = useStore()
// 路由
import { useRouter } from 'vue-router';

const router = useRouter()

//控制注册与登录表单的显示， 默认显示用户登录
const isRegister = ref(true)
const roleType = ref("user")
const loginForm = ref(null)
const registerForm = ref(null)
//控制全局登录
const loginInfo=ref({
    loginType:'',  //用户类型：用户，商家，骑手
    loginId:null,
    loginPassword:'',
})

//定义数据模型
const adminData = ref({
    AdminId:null,
    Password:'',
})
const userRegisterData = ref({
    userId:null,
    username:'',
    phoneNumber:'',
    password:'',
    rePassword:'',
    Wallet:0,
    WalletPassword:'',
    reWalletPassword:'',
})
const merchantRegisterData = ref({
    MerchantId:null,
    Password:'',
    MerchantName:'',
    MerchantAddress:'',
    Contact:'',
    DishType:'',
    CouponType:0,
    TimeforOpenBusiness: '',
    TimeforCloseBusiness: '',
    rePassword:'',
    Wallet:0,
    WalletPassword:'',
    reWalletPassword:'',
})
const riderRegisterData = ref({
    RiderId:null,
    Password:'',
    RiderName:'',
    PhoneNumber:'',
    rePassword:'',
    Wallet:0,
    WalletPassword:'',
    reWalletPassword:'',
})


const clearLoginData = () =>{
    loginForm.value.clearValidate('loginId');
    loginForm.value.clearValidate('loginPassword');
    loginForm.value.clearValidate('loginType');
    loginInfo.value.loginId = null;
    loginInfo.value.loginPassword = '';
}

//定义函数，清空数据模型
const clearRegisterData = () =>{
    // 除去验证结果
    registerForm.value.clearValidate();
    
    userRegisterData.value = {
        userId:null,
        username:'',
        phoneNumber:'',
        password:'',
        rePassword:'',
        Wallet:0,
        WalletPassword:'',
        reWalletPassword:'',
    },
    merchantRegisterData.value = {
        MerchantId:null,
        Password:'',
        MerchantName:'',
        MerchantAddress:'',
        Contact:'',
        DishType:'',
        CouponType:0,
        TimeforOpenBusiness: '',
        TimeforCloseBusiness: '',
        rePassword:'',
        Wallet:0,
        WalletPassword:'',
        reWalletPassword:'',
    },
    riderRegisterData.value = {
        RiderId:null,
        Password:'',
        RiderName:'',
        PhoneNumber:'',
        rePassword:'',
        Wallet:0,
        WalletPassword:'',
        reWalletPassword:'',
    },
    // loginInfo.value = {
    //     loginType:'',  
    //     loginId:null,
    //     loginPassword:'',
    // },
    adminData.value={}
}

//二次校验密码的函数
const checkRePassword = (rule,value,callback) => {
    if(value == ''){
        callback(new Error('请再次确认密码'))
    } else if( roleType.value === 'user' && value !== userRegisterData.value.password){
        callback('二次确认密码不相同请重新输入')
    } else if( roleType.value ==='merchant' && value !== merchantRegisterData.value.Password){
        callback('二次确认密码不相同请重新输入')
    }else if( roleType.value === 'rider' && value !== riderRegisterData.value.Password){
        callback('二次确认密码不相同请重新输入')
    }
    else{
        callback()
    }
}
const checkWalletRePassword = (rule,value,callback) => {
    if(value == ''){
        callback(new Error('请再次确认钱包密码'))
    } else if( roleType.value === 'user' &&value !== userRegisterData.value.WalletPassword){
        callback('二次确认钱包密码不相同请重新输入')
    } else if( roleType.value ==='merchant' && value !== merchantRegisterData.value.WalletPassword){
        callback('二次确认钱包密码不相同请重新输入')
    }else if( roleType.value === 'rider' && value !== riderRegisterData.value.WalletPassword){
        callback('二次确认钱包密码不相同请重新输入')
    }
    else{
        callback()
    }
}

 
//定义表单校验规则
const userRules = ref({
    username:[
        {required:true, message:'请输入用户名', trigger:'blur'},
        {min:5, max:16, message:'请输入长度5~16非空字符', trigger:'blur'}
    ],
    phoneNumber:[
        {required:true, message:'请输入手机号码', trigger:'blur'},
        {  
            pattern: /^[0-9]{11}$/, // 确保输入是长度为11的数字  
            message: '电话号码必须是11位数字', // 错误提示消息  
            trigger: 'blur', 
        },  
    ],
    password:[
        {required:true, message:'请输入密码', trigger:'blur'},
        {min:5, max:16, message:'请输入长度5~16非空字符', trigger:'blur'}
    ],
    WalletPassword:[
        {required:true, message:'请输入支付密码', trigger:'blur'},
        {  
            pattern: /^[0-9]{6}$/, 
            message: '支付密码必须是6位数字', // 错误提示消息  
            trigger: 'blur', 
        },  
    ],
    rePassword:[{validator:checkRePassword,trigger:'blur'}], //校验二次输入密码是否相同
    reWalletPassword: [{validator:checkWalletRePassword,trigger:'blur'}]
})
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
    Password: [{ required: true, message: '请输入密码', trigger: 'blur' }, { min: 5, max: 16, message: '请输入长度5~16非空字符', trigger: 'blur' }],  
    rePassword: [{ validator: checkRePassword, trigger: 'blur' }], 
    TimeforOpenBusiness: [{ required: true, message: '请选择营业开始时间', trigger: 'change' }],  
    TimeforCloseBusiness: [{ required: true, message: '请选择营业结束时间', trigger: 'change' }],
    DishType: [{ required: true, message: '请输入菜品类型', trigger: 'blur' }], 
    WalletPassword:[
        {required:true, message:'请输入支付密码', trigger:'blur'},
        {  
            pattern: /^[0-9]{6}$/, // 确保输入是长度为11的数字  
            message: '支付密码必须是6位数字', // 错误提示消息  
            trigger: 'blur', 
        },  
    ], 
    reWalletPassword: [{validator:checkWalletRePassword,trigger:'blur'}]
});
const riderRules = ref({
    RiderName:[
        {required:true, message:'请输入骑手姓名', trigger:'blur'},
        {min:2, max:16, message:'请输入长度2~16非空字符', trigger:'blur'}
    ],
    PhoneNumber:[
        {required:true, message:'请输入手机号码', trigger:'blur'},
        {  
            pattern: /^[0-9]{11}$/, // 确保输入是长度为11的数字  
            message: '电话号码必须是11位数字', // 错误提示消息  
            trigger: 'blur', 
        },  
    ],
    Password:[
        {required:true, message:'请输入密码', trigger:'blur'},
        {min:3, max:16, message:'请输入长度5~16非空字符', trigger:'blur'}
    ],
    WalletPassword:[
        {required:true, message:'请输入支付密码', trigger:'blur'},
        {  
            pattern: /^[0-9]{6}$/, 
            message: '支付密码必须是6位数字', // 错误提示消息  
            trigger: 'blur', 
        },  
    ],
    rePassword:[{validator:checkRePassword,trigger:'blur'}], //校验二次输入密码是否相同
    reWalletPassword: [{validator:checkWalletRePassword,trigger:'blur'}]
});
const loginRules = ref({
    loginType:[
        {required:true, message:'请选择用户类型', trigger:'blur'},
    ],
    loginId:[
        {required:true, message:'请输入用户Id', trigger:'blur'},
    ],
    loginPassword:[
        {required:true, message:'请输入密码', trigger:'blur'},
        {min:5, max:16, message:'请输入长度5~16非空字符', trigger:'blur'}
    ]
})
//调用后台接口完成注册
import {userRegisterService, userLoginService} from '@/api/user.js'
import {merchantRegisterService, merchantLoginService,assignStationToMerchant,AssignStation} from '@/api/merchant.js'
import {riderRegisterService, riderLoginService} from '@/api/rider.js'
import {adminLoginService} from '@/api/platform.js'
const register = ()=> {
     // 先验证数据

    registerForm.value.validate((valid,fields) => {
        console.log(1);
        if (!valid) {
            console.log(fields);
            return false;
        }
        console.log(2);
        if (roleType.value === 'merchant') {
            // 转换时间为秒数
            const openTime = merchantRegisterData.value.TimeforOpenBusiness;  
            const closeTime = merchantRegisterData.value.TimeforCloseBusiness;
            const startHour = openTime.getHours();  
            const startMinute = openTime.getMinutes();  
            const startSecond = openTime.getSeconds();  
            merchantRegisterData.value.TimeforOpenBusiness = startHour * 3600 + startMinute * 60 + startSecond;  

            const endHour = closeTime.getHours();  
            const endMinute = closeTime.getMinutes();  
            const endSecond = closeTime.getSeconds();  
            merchantRegisterData.value.TimeforCloseBusiness = endHour * 3600 + endMinute * 60 + endSecond;
            merchantRegisterService({
                Password: merchantRegisterData.value.Password,  
                MerchantName: merchantRegisterData.value.MerchantName,    
                MerchantAddress: merchantRegisterData.value.MerchantAddress,  
                Contact: merchantRegisterData.value.Contact,  
                DishType: merchantRegisterData.value.DishType,  
                CouponType: merchantRegisterData.value.CouponType,  
                TimeforOpenBusiness: merchantRegisterData.value.TimeforOpenBusiness,  
                TimeforCloseBusiness: merchantRegisterData.value.TimeforCloseBusiness,  
                WalletPassword: merchantRegisterData.value.WalletPassword,
                Wallet:0,  
            }).then(res => { 
                assignStationToMerchant(merchantRegisterData.value.MerchantAddress).then(response=>{
                const stationId=response;
                const data={
                    MerchantId:res.data,
                    StationId:stationId,
                }
                AssignStation(data).then(data=>{
                    console.log(data);
                }).catch(error => {
                    console.log(error);
                });
            }).catch(error => {
                console.log(error);
            });
                router.push('/login')   
                ElMessage.success({  
                    message: '商家注册成功，商家ID为 ' + res.data + '，请牢记该Id。',  
                    duration: 10000 // 设置显示时间为10秒  
                });   
                clearRegisterData();  
            }).catch(err => {  
                ElMessage.error(`注册失败: ${err.response.data.msg || '未知错误'}`);  
            });
        } else if (roleType.value === 'rider') {
            riderRegisterService({  
                RiderName: riderRegisterData.value.RiderName, // 后端要求的字段  
                Password: riderRegisterData.value.Password,  
                PhoneNumber: riderRegisterData.value.PhoneNumber,  
                WalletPassword: riderRegisterData.value.WalletPassword,
                Wallet:0,
            }).then(res => {  
                // 成功  
                ElMessage.success({  
                    message: '骑手注册成功，骑手ID为 ' + res.data + '，请牢记该Id。',  
                    duration: 10000 // 设置显示时间为10秒  
                });  
                clearRegisterData() // 注册成功后清空输入  
                // 可选择跳转到其他页面，例如登录 page  
                router.push('/login')  
            }).catch(err => {  
                // 处理错误  
                ElMessage.error(`注册失败: ${err.response.data || '未知错误'}`)  
            })
        } else {
       // 调用注册接口  
            userRegisterService({  
                UserName: userRegisterData.value.username, // 后端要求的字段  
                Password: userRegisterData.value.password,  
                PhoneNumber: userRegisterData.value.phoneNumber,  
                WalletPassword: userRegisterData.value.WalletPassword,
                Wallet:0,
            }).then(res => {  
                // 成功  
                ElMessage.success({  
                    message: '用户注册成功，用户ID为 ' + res.data + '，请牢记该Id。',  
                    duration: 10000 // 设置显示时间为10秒  
                });  
                clearRegisterData() // 注册成功后清空输入  
                // 可选择跳转到其他页面，例如登录 page  
                router.push('/login')  
            }).catch(err => {  
                // 处理错误  
                ElMessage.error(`注册失败: ${err.response.data || '未知错误'}`)  
            })
        }  
    }); 
}

const login = () =>{  //登录
    // 先验证数据
    loginForm.value.validate((valid) => {
        if (!valid) {
            return false;
        }
        
        const loginTypeValue = loginInfo.value.loginType;
        if (loginTypeValue === 'user') {  //用户登录
            //调用接口完成登录
            userRegisterData.value.userId = loginInfo.value.loginId;
            userRegisterData.value.password = loginInfo.value.loginPassword;
            userLoginService(userRegisterData.value).then(data => {
                if(data.msg=== "ok"){
                    ElMessage.success('登录成功');
                    // 将用户信息保存到管理器
                    store.dispatch('setUser', userRegisterData.value); // 设置用户状态
                    router.push('/user-home');
                }
            }).catch(error => {
                // 根据错误码处理不同的错误  
                if (error.response && error.response.data) {  
                    const errorCode = error.response.data.errorCode;  

                    if (errorCode === 20000) {  
                        ElMessage.error('用户Id或密码错误');  
                    } else if (errorCode === 30000) {  
                        ElMessage.error('登录异常: ' + error.response.data.msg);  
                    } else {  
                        ElMessage.error('发生未知错误');  
                    }  
                } else {  
                     ElMessage.error('网络错误，请重试');  
                }  
                store.state.user = null;  
                //cookie.set('user', {});  
            });
        } else if (loginTypeValue === 'merchant') {  //商家登录
            //调用接口完成登录
            merchantRegisterData.value.MerchantId = +loginInfo.value.loginId;
            merchantRegisterData.value.Password = loginInfo.value.loginPassword;
            merchantRegisterData.value.TimeforOpenBusiness = +merchantRegisterData.value.TimeforOpenBusiness;
            merchantRegisterData.value.TimeforCloseBusiness = +merchantRegisterData.value.TimeforCloseBusiness;
            merchantLoginService(merchantRegisterData.value).then(data => {
                if(data.msg=== "ok"){
                    ElMessage.success('登录成功');
                    store.dispatch('setMerchant', merchantRegisterData.value); // 设置商家状态
                    router.push('/merchant-home');
                }
            }).catch(error => {
                // 根据错误码处理不同的错误  
                if (error.response && error.response.data) {  
                    const errorCode = error.response.data.errorCode;  

                    if (errorCode === 20000) {  
                        ElMessage.error('商家Id或密码错误');  
                    } else if (errorCode === 30000) {  
                        ElMessage.error('登录异常: ' + error.response.data.msg);  
                    } else {  
                        ElMessage.error('发生未知错误');  
                    }  
                } else {  
                     ElMessage.error('网络错误，请重试');  
                }  

                store.state.merchant = null;  
                //cookie.set('merchant', {});  
            });
            
        } else if(loginTypeValue === 'rider') {  
            riderRegisterData.value.RiderId = loginInfo.value.loginId;
            riderRegisterData.value.Password = loginInfo.value.loginPassword;
            riderLoginService(riderRegisterData.value).then(data => {
                if(data.msg=== "ok"){
                    ElMessage.success('登录成功');
                    // 将用户信息保存到管理器
                    store.dispatch('setRider', riderRegisterData.value); // 设置用户状态
                    router.push('/rider-home/assignment');
                }
            }).catch(error => {
                // 根据错误码处理不同的错误  
                if (error.response && error.response.data) {  
                    const errorCode = error.response.data.errorCode;  

                    if (errorCode === 20000) {  
                        ElMessage.error('骑手Id或密码错误');  
                    } else if (errorCode === 30000) {  
                        ElMessage.error('登录异常: ' + error.response.data.msg);  
                    } else {  
                        ElMessage.error('发生未知错误');  
                    }  
                } else {  
                     ElMessage.error('网络错误，请重试');  
                }  
                store.state.rider = null;  
            });
        }else{
            adminData.value.AdminId = loginInfo.value.loginId;
            adminData.value.Password = loginInfo.value.loginPassword;
            adminLoginService(adminData.value).then(data => {
                if(data.msg=== "ok"){
                    ElMessage.success('登录成功');
                    // 将用户信息保存到管理器
                    store.dispatch('setAdmin', adminData.value); // 设置用户状态
                    console.log(store.state.admin);
                    router.push('/platform-home');
                }
            }).catch(error => {
                // 根据错误码处理不同的错误  
                if (error.response && error.response.data) {  
                    const errorCode = error.response.data.errorCode;  
                    if (errorCode === 20000) {  
                        ElMessage.error('管理员Id或密码错误');  
                    } else if (errorCode === 30000) {  
                        ElMessage.error('登录异常: ' + error.response.data.msg);  
                    } else {  
                        ElMessage.error('发生未知错误');  
                    }  
                } else {  
                     ElMessage.error('网络错误，请重试');  
                }  
                store.state.admin = null;  
            });
        }  
    });
}

//切换界面
const mySwitch = () => {
    const pre_box = document.querySelector('.pre-box')
    const img = document.querySelector("#avatar")
    if (isRegister.value) {
        pre_box.style.transform = "translateX(100%)"
        pre_box.style.backgroundColor = "#c9e0ed"
        clearRegisterData()
    }
    else {
        pre_box.style.transform = "translateX(0%)"
        pre_box.style.backgroundColor = "#edd4dc"
        clearLoginData()
    }
    isRegister.value = !isRegister.value
}
</script>
 
<template>
    <!-- 最外层的背景盒子 -->
    <div class="background">
            <!-- 登录和注册盒子 -->
        <div class="loginAndRegister" ref="loginAndRegister">
            <!-- 滑动盒子 -->
            <div class="pre-box">
                <h1>WELCOME</h1>
                <p>JOIN US!</p>
                <div class="img-box">
                    <img src="@/assets/index_cover.png" alt="" id="avatar">
                </div>
            </div>
            <!-- 注册盒子 -->
            <div class="register">
                <!-- 标题盒子 -->
                <div class="title">
                    <h1>{{ roleType === 'merchant' ? '商家注册' : roleType === 'rider' ? '骑手注册' : '用户注册' }}</h1>
                </div>



                <!-- 输入框 -->
                <el-form ref="registerForm" size="large" autocomplete="off" 
                     :model="roleType === 'merchant' ? merchantRegisterData : roleType === 'rider' ? riderRegisterData : userRegisterData"   
                     :rules="roleType === 'merchant' ? merchantRules : roleType === 'rider' ? riderRules : userRules">        
                    <!-- 选项框 -->
                    <el-radio-group v-model="roleType">  
                        <el-radio label="user" @change="clearRegisterData();">用户注册</el-radio>  
                        <el-radio label="merchant" @change="clearRegisterData();">商家注册</el-radio>  
                        <el-radio label="rider" @change="clearRegisterData();">骑手注册</el-radio>  
                    </el-radio-group>
                    <!--商家注册--> 
                    <el-form-item v-if="roleType === 'merchant'" prop="MerchantName">  
                        <el-input :prefix-icon="User" placeholder="请输入商家名称" v-model="merchantRegisterData.MerchantName"></el-input>  
                    </el-form-item>  
                    <el-form-item v-if="roleType === 'merchant'" prop="MerchantAddress">  
                        <el-input :prefix-icon="User" placeholder="请输入商家地址" v-model="merchantRegisterData.MerchantAddress"></el-input>  
                    </el-form-item>  
                    <el-form-item v-if="roleType === 'merchant'" prop="Contact">  
                        <el-input :prefix-icon="User" placeholder="请输入联系方式" v-model="merchantRegisterData.Contact"></el-input>  
                    </el-form-item>
                    <el-form-item v-if="roleType === 'merchant'" prop="DishType">  
                        <el-input :prefix-icon="User" placeholder="请输入菜品类型" v-model="merchantRegisterData.DishType"></el-input>  
                    </el-form-item>
                    <div class="startandend">
                        <el-form-item class="starttime" v-if="roleType === 'merchant'" prop="TimeforOpenBusiness">  
                            <el-time-picker placeholder="请选择营业开始时间" v-model="merchantRegisterData.TimeforOpenBusiness" :picker-options="{ selectableRange: '00:00:00 - 23:59:59' }"></el-time-picker>  
                        </el-form-item>  
                        <el-form-item class="endtime" v-if="roleType === 'merchant'" prop="TimeForCloseBusiness">  
                            <el-time-picker placeholder="请选择营业结束时间" v-model="merchantRegisterData.TimeforCloseBusiness" :picker-options="{ selectableRange: '00:00:00 - 23:59:59' }"></el-time-picker>  
                        </el-form-item>  
                    </div>
                    <el-form-item v-if="roleType === 'merchant'" prop="Password">
                        <el-input :prefix-icon="Lock" show-password  placeholder="请输入密码" v-model="merchantRegisterData.Password"></el-input>
                    </el-form-item>
                    <el-form-item v-if="roleType === 'merchant'" prop="rePassword">
                        <el-input :prefix-icon="Lock" show-password  placeholder="请再次输入密码" v-model="merchantRegisterData.rePassword"></el-input>
                    </el-form-item>
                    <el-form-item v-if="roleType === 'merchant'" prop="WalletPassword">
                        <el-input :prefix-icon="Lock" show-password  placeholder="请输入支付密码" v-model="merchantRegisterData.WalletPassword"></el-input>
                    </el-form-item>
                    <el-form-item v-if="roleType === 'merchant'" prop="reWalletPassword">
                        <el-input :prefix-icon="Lock" show-password  placeholder="请再次输入支付密码" v-model="merchantRegisterData.reWalletPassword"></el-input>
                    </el-form-item>
                    <!--用户注册-->    
                    <el-form-item v-if="roleType === 'user'" prop="username">
                        <el-input :prefix-icon="User" placeholder="请输入用户名" v-model="userRegisterData.username"></el-input>
                    </el-form-item>
                    <el-form-item v-if="roleType === 'user'" prop="phoneNumber">
                        <el-input :prefix-icon="User" placeholder="请输入手机号码" v-model="userRegisterData.phoneNumber"></el-input>
                    </el-form-item>
                    <el-form-item v-if="roleType === 'user'" prop="password">
                        <el-input :prefix-icon="Lock" show-password placeholder="请输入密码" v-model="userRegisterData.password"></el-input>
                    </el-form-item>
                    <el-form-item v-if="roleType === 'user'" prop="rePassword">
                        <el-input :prefix-icon="Lock" show-password placeholder="请再次输入密码" v-model="userRegisterData.rePassword"></el-input>
                    </el-form-item>
                    <el-form-item v-if="roleType === 'user'" prop="WalletPassword">
                        <el-input :prefix-icon="Lock" show-password placeholder="请输入支付密码" v-model="userRegisterData.WalletPassword"></el-input>
                    </el-form-item>
                    <el-form-item v-if="roleType === 'user'" prop="reWalletPassword">
                        <el-input :prefix-icon="Lock" show-password placeholder="请再次输入支付密码" v-model="userRegisterData.reWalletPassword"></el-input>
                    </el-form-item>
                    <!--骑手注册-->  
                    <el-form-item v-if="roleType === 'rider'" prop="RiderName">
                        <el-input :prefix-icon="User" placeholder="请输入骑手姓名" v-model="riderRegisterData.RiderName"></el-input>
                    </el-form-item>
                    <el-form-item v-if="roleType === 'rider'" prop="PhoneNumber">
                        <el-input :prefix-icon="User" placeholder="请输入手机号码" v-model="riderRegisterData.PhoneNumber"></el-input>
                    </el-form-item>
                    <el-form-item v-if="roleType === 'rider'" prop="Password">
                        <el-input :prefix-icon="Lock" show-password placeholder="请输入密码" v-model="riderRegisterData.Password"></el-input>
                    </el-form-item>
                    <el-form-item v-if="roleType === 'rider'" prop="rePassword">
                        <el-input :prefix-icon="Lock" show-password placeholder="请再次输入密码" v-model="riderRegisterData.rePassword"></el-input>
                    </el-form-item>
                    <el-form-item v-if="roleType === 'rider'" prop="WalletPassword">
                        <el-input :prefix-icon="Lock" show-password placeholder="请输入支付密码" v-model="riderRegisterData.WalletPassword"></el-input>
                    </el-form-item>
                    <el-form-item v-if="roleType === 'rider'" prop="reWalletPassword">
                        <el-input :prefix-icon="Lock" show-password placeholder="请再次输入支付密码" v-model="riderRegisterData.reWalletPassword"></el-input>
                    </el-form-item>
                </el-form>

                <!-- 按钮盒子 -->
                <div class="btn-box">
                    <!-- 绑定注册事件 -->
                    <button @click="register">注册</button>
                    <!-- 绑定点击事件 -->
                    <p @click="mySwitch">已有账号?去登录</p>
                </div>
            </div>
                
            <!-- 登录盒子 -->
            <div class="login">
                <!-- 标题盒子 -->
                <div class="title">
                    <h1>登录</h1>
                </div>
                <!-- 输入框盒子 -->
                <el-form ref="loginForm"  autocomplete="off" :model="loginInfo" :rules="loginRules">
                    <el-form-item prop="loginType">  
                        <el-select v-model="loginInfo.loginType" placeholder="选择用户类型" @change="clearLoginData();">  
                            <el-option label="用户" value="user"></el-option>  
                            <el-option label="商家" value="merchant"></el-option>  
                            <el-option label="骑手" value="rider"></el-option>  
                            <el-option label="管理员" value="admin"></el-option>  
                        </el-select>  
                    </el-form-item>
                    <el-form-item prop="loginId">
                        <el-input :prefix-icon="User" placeholder="请输入用户Id" v-model="loginInfo.loginId"></el-input>
                    </el-form-item>
                    <el-form-item prop="loginPassword">
                        <el-input name="password" :prefix-icon="Lock" show-password placeholder="请输入密码" v-model="loginInfo.loginPassword"></el-input>
                    </el-form-item>
                </el-form>
                <!-- 按钮盒子 -->
                <div class="btn-box">
                    <!-- 绑定登录事件 -->
                    <button @click="login">登录</button>
                    <!-- 绑定点击事件 -->
                    <p @click="mySwitch">没有账号?去注册</p>
                </div>
            </div>
        </div>
    </div>
</template>
 
<style scoped>
input {
    outline: none;
}

.background {
    margin:0;
    height: 100vh;
    overflow-x: hidden;
    display: flex;
    /* 渐变方向从左到右 */
    background: linear-gradient(to right, rgb(247, 209, 215), rgb(191, 227, 241));
    z-index: 1;
}

.loginAndRegister {
    width: 1200px;
    height: 95vh;
    display: flex;
    /* 相对定位 */
    position: relative;
    z-index: 2;
    margin: auto;
    /* 设置圆角 */
    border-radius: 8px;
    /* 设置边框 */
    border: 1px solid rgba(255, 255, 255, 0.6);
    /* 设置盒子阴影 */
    box-shadow: 2px 1px 19px rgba(0, 0, 0, 0.1);
}

/* 滑动的盒子 */
.pre-box {
  /* 宽度为大盒子的一半 */
  width: 50%;
  height: 100%;
  /* 绝对定位 */
  position: absolute;
  /* 距离大盒子左侧为0 */
  left: 0;
  /* 距离大盒子顶部为0 */
  top: 0;
  z-index: 99;
  border-radius: 4px;
  background-color: #edd4dc;
  box-shadow: 2px 1px 19px rgba(0, 0, 0, 0.1);
  /* 动画过渡，先加速再减速 */
  transition: 0.5s ease-in-out;
}

/* 滑动盒子的标题 */
.pre-box h1 {
  margin-top: 150px;
  text-align: center;
  /* 文字间距 */
  letter-spacing: 5px;
  color: white;
  /* 禁止选中 */
  user-select: none;
  /* 文字阴影 */
  text-shadow: 4px 4px 3px rgba(0, 0, 0, 0.1);
}

/* 滑动盒子的文字 */
.pre-box p {
  height: 30px;
  line-height: 30px;
  text-align: center;
  margin: 20px 0;
  /* 禁止选中 */
  user-select: none;
  font-weight: bold;
  color: white;
  text-shadow: 4px 4px 3px rgba(0, 0, 0, 0.1);
}

/* 图片盒子 */
.img-box {
  width: 300px;
  height: 200px;
  margin: 20px auto;
  /* 设置为圆形 */
  border-radius: 50%;
  /* 设置用户禁止选中 */
  user-select: none;
  overflow: hidden;
  box-shadow: 4px 4px 3px rgba(0, 0, 0, 0.1);
}

/* 图片 */
.img-box img {
  width: 100%;
  height: 100%; /* 确保图片占满整个盒子的高度 */
  transition: 0.5s;
}

/* 登录和注册盒子 */
.login {
  flex: 1;
  height: 100%;
  display:grid;
}

.register {
  flex: 1;
  height: 100%;
  display:grid;
}

/* 标题盒子 */
.title {
  height: 100px;
  line-height: 50px;
}

/* 标题 */
.title h1 {
  text-align: center;
  color: white;
  /* 禁止选中 */
  user-select: none;
  letter-spacing: 5px;
  text-shadow: 4px 4px 3px rgba(0, 0, 0, 0.1);
}

/* 输入框盒子 */
.el-form {
  display: flex;
  /* 纵向布局 */
  flex-direction: column;
  /* 水平居中 */
  align-items: center;
}
.el-form-item {
  width: 65%;
  height: 40px;
}

/* 输入框 */
input {
  /* width: 60%; */
  height: 40px;
  margin-bottom: 20px;
  text-indent: 10px;
  border: 1px solid #fff;
  background-color: rgba(255, 255, 255, 0.3);
  border-radius: 120px;
  /* 增加磨砂质感 */
  backdrop-filter: blur(10px);
}

input:focus {
  /* 光标颜色 */
  color: #b0cfe9;
}

/* 聚焦时隐藏文字 */
input:focus::placeholder {
  opacity: 0;
}

/* 按钮盒子 */
.btn-box {
  display: flex;
  justify-content: center;
}

/* 按钮 */
button {
  width: 100px;
  height: 40px;
  margin: 0 7px;
  text-align: center;
  border: none;
  border-radius: 4px;
  background-color: #69b3f0;
  color: white;
}

/* 按钮悬停时 */
button:hover {
  /* 鼠标小手 */
  cursor: pointer;
  /* 透明度 */
  opacity: 0.8;
}

/* 按钮文字 */
.btn-box p {
  height: 30px;
  line-height: 30px;
  /* 禁止选中 */
  user-select: none;
  font-size: 14px;
  color: white;
}

.btn-box p:hover {
  cursor: pointer;
  border-bottom: 1px solid white;
}

.startandend {
    width: 65%;
    display:flex;
    align-items: center;
    justify-content: space-between;
}

.starttime {
    width: 175px;
}

.endtime {
    width: 175px;
}

</style>