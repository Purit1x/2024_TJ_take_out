<script setup>
// 导入element-plus的提示组件
import { ElMessage } from 'element-plus'
// 导入用户图标和锁图标
import { User, Lock, Dish } from '@element-plus/icons-vue'
// 导入引用
import { ref } from 'vue'
// 状态管理
import { useStore } from "vuex"
const store = useStore()
// 使用cookie
import { useCookies } from '@vueuse/integrations/useCookies'
const cookie = useCookies()
// 路由
import { useRouter } from 'vue-router';
const router = useRouter()

//控制注册与登录表单的显示， 默认显示用户注册
const isRegister = ref(false)
const roleType = ref("user")
const refForm =ref(null);

//控制全局登录
const loginInfo=ref({
    loginType:'',  //用户类型：用户，商家，骑手
    loginId:null,
    loginPassword:'',
})


//定义数据模型
const userRegisterData = ref({
    userId:null,
    username:'',
    phoneNumber:'',
    password:'',
    rePassword:'',
})
const merchantRegisterData = ref({
    MerchantId:null,
    Password:'',
    MerchantName:'',
    MerchantAddress:'',
    Contact:'',
    DishType:'',
    TimeforOpenBusiness: '',
    TimeforCloseBusiness: '',
    rePassword:'',
})
const riderRegisterData = ref({
    riderId:null,
    riderName:'',
    phoneNumber:'',
    password:'',
    rePassword:'',
})


//定义函数，清空数据模型
const clearRegisterData = () =>{
    // 除去验证结果
    refForm.value.clearValidate();
    
    userRegisterData.value = {
        userId:null,
        username:'',
        phoneNumber:'',
        password:'',
        rePassword:'',
    },
    merchantRegisterData.value = {
        MerchantId:null,
        Password:'',
        MerchantName:'',
        MerchantAddress:'',
        Contact:'',
        DishType:'',
        TimeforOpenBusiness: '',
        TimeforCloseBusiness: '',
        rePassword:'',
    },
    riderRegisterData.value = {
        riderId:null,
        riderName:'',
        phoneNumber:'',
        password:'',
        rePassword:'',
    },
    loginInfo.value = {
        loginType:'',  
        loginId:null,
        loginPassword:'',
    }

}

//二次校验密码的函数
const checkRePassword = (rule,value,callback) => {
    if(value == ''){
        callback(new Error('请再次确认密码'))
    } else if( roleType.value === 'user' && value !== userRegisterData.value.password){
        callback('二次确认密码不相同请重新输入')
    } else if( roleType.value ==='merchant' && value !== merchantRegisterData.value.Password){
        callback('二次确认密码不相同请重新输入')
    }else if( roleType.value === 'rider' && value !== riderRegisterData.value.password){
        callback('二次确认密码不相同请重新输入')
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
    ],
    password:[
        {required:true, message:'请输入密码', trigger:'blur'},
        {min:5, max:16, message:'请输入长度5~16非空字符', trigger:'blur'}
    ],
    rePassword:[{validator:checkRePassword,trigger:'blur'}] //校验二次输入密码是否相同
})
const merchantRules = ref({  
    MerchantName: [{ required: true, message: '请输入商家名称', trigger: 'blur' }],  
    MerchantAddress: [{ required: true, message: '请填写商家地址', trigger: 'blur' }],  
    Contact: [{ required: true, message: '请输入联系方式', trigger: 'blur' }],  
    Password: [{ required: true, message: '请输入密码', trigger: 'blur' }, { min: 5, max: 16, message: '请输入长度5~16非空字符', trigger: 'blur' }],  
    rePassword: [{ validator: checkRePassword, trigger: 'blur' }], 
    TimeforOpenBusiness: [{ required: true, message: '请选择营业开始时间', trigger: 'change' }],  
    TimeforCloseBusiness: [{ required: true, message: '请选择营业结束时间', trigger: 'change' }],
    DishType: [{ required: true, message: '请输入菜品类型', trigger: 'blur' }]  
});
const riderRules = ref({
    riderName:[
        {required:true, message:'请输入用户名', trigger:'blur'},
        {min:5, max:16, message:'请输入长度5~16非空字符', trigger:'blur'}
    ],
    phoneNumber:[
        {required:true, message:'请输入手机号码', trigger:'blur'},
    ],
    password:[
        {required:true, message:'请输入密码', trigger:'blur'},
        {min:5, max:16, message:'请输入长度5~16非空字符', trigger:'blur'}
    ],
    rePassword:[{validator:checkRePassword,trigger:'blur'}] //校验二次输入密码是否相同
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
import {merchantRegisterService, merchantLoginService} from '@/api/merchant.js'
import {riderRegisterService, riderLoginService} from '@/api/rider.js'
import { start } from 'nprogress';
const register = ()=> {
     // 先验证数据
    refForm.value.validate((valid) => {
        if (!valid) {
            return false;
        }
        if (roleType.value === 'merchant') {
            // 转换时间为秒数
            const openTime = merchantRegisterData.value.TimeforOpenBusiness;  
            const closeTime = merchantRegisterData.value.TimeforCloseBusiness;
            const startHour = openTime.getUTCHours()-16;  
            const startMinute = openTime.getUTCMinutes();  
            const startSecond = openTime.getUTCSeconds();  
            merchantRegisterData.value.TimeforOpenBusiness = startHour * 3600 + startMinute * 60 + startSecond;  

            const endHour = closeTime.getUTCHours()-16;  
            const endMinute = closeTime.getUTCMinutes();  
            const endSecond = closeTime.getUTCSeconds();  
            merchantRegisterData.value.TimeforCloseBusiness = endHour * 3600 + endMinute * 60 + endSecond;
            merchantRegisterService({
                Password: merchantRegisterData.value.Password,  
                MerchantName: merchantRegisterData.value.MerchantName,    
                MerchantAddress: merchantRegisterData.value.MerchantAddress,  
                Contact: merchantRegisterData.value.Contact,  
                DishType: merchantRegisterData.value.DishType,  
                TimeforOpenBusiness: merchantRegisterData.value.TimeforOpenBusiness,  
                TimeforCloseBusiness: merchantRegisterData.value.TimeforCloseBusiness,  
            }).then(res => {  
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
                UserName: riderRegisterData.value.riderName,
                Password: riderRegisterData.value.password,
                PhoneNumber: riderRegisterData.value.phoneNumber  
            }).then(res => {
                // 成功  
                ElMessage.success({  
                    message: '骑手注册成功，骑手ID为 ' + res.data + '，请牢记该Id。',  
                    duration: 10000 // 设置显示时间为10秒  
                });  
                clearRegisterData() // 注册成功后清空输入  
                router.push('/login') 
            }).catch(err => {  
                ElMessage.error(`注册失败: ${err.response.data.msg || '未知错误'}`);  
            });
        } else {
       // 调用注册接口  
            userRegisterService({  
                UserName: userRegisterData.value.username, // 后端要求的字段  
                Password: userRegisterData.value.password,  
                PhoneNumber: userRegisterData.value.phoneNumber  
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
                ElMessage.error(`注册失败: ${err.response.data.msg || '未知错误'}`)  
            })
        }  
    }); 
}

const login = () =>{
    // 先验证数据
    refForm.value.validate((valid) => {
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
                    // 保持cookie
                    cookie.set('user', userRegisterData.value);
                    // 跳转
                    router.push('/user-home');
                }
            }).catch(error => {
                // 根据错误码处理不同的错误  
                if (error.response && error.response.data) {  
                    const errorCode = error.response.data.errorCode;  

                    if (errorCode === 20000) {  
                        ElMessage.error('用户名或密码错误');  
                    } else if (errorCode === 30000) {  
                        ElMessage.error('登录异常: ' + error.response.data.msg);  
                    } else {  
                        ElMessage.error('发生未知错误');  
                    }  
                } else {  
                     ElMessage.error('网络错误，请重试');  
                }  
                store.state.user = null;  
                cookie.set('user', {});  


            });
        } else if (loginTypeValue === 'rider') {  //骑手登录
            //调用接口完成登录
            riderRegisterData.value.userId = loginInfo.value.loginId;
            riderRegisterData.value.password = loginInfo.value.loginPassword;
            riderLoginService(riderRegisterData.value).then(data => {
                if(data.msg=== "ok"){
                    ElMessage.success('登录成功');
                    // 将骑手信息保存到管理器
                    store.dispatch('setRider', riderRegisterData.value); // 设置骑手状态
                    // 保持cookie
                    cookie.set('rider', riderRegisterData.value);
                    // 跳转
                    router.push('/rider-home');
                }
            }).catch(error => {
                // 根据错误码处理不同的错误  
                if (error.response && error.response.data) {  
                    const errorCode = error.response.data.errorCode;  

                    if (errorCode === 20000) {  
                        ElMessage.error('用户名或密码错误');  
                    } else if (errorCode === 30000) {  
                        ElMessage.error('登录异常: ' + error.response.data.msg);  
                    } else {  
                        ElMessage.error('发生未知错误');  
                    }  
                } else {  
                     ElMessage.error('网络错误，请重试');  
                }  
                store.state.rider = null;  
                cookie.set('rider', {});  

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
                    cookie.set('merchant', merchantRegisterData.value);
                    router.push('/merchant-home');
                }
            }).catch(error => {
                // 根据错误码处理不同的错误  
                if (error.response && error.response.data) {  
                    const errorCode = error.response.data.errorCode;  

                    if (errorCode === 20000) {  
                        ElMessage.error('用户名或密码错误');  
                    } else if (errorCode === 30000) {  
                        ElMessage.error('登录异常: ' + error.response.data.msg);  
                    } else {  
                        ElMessage.error('发生未知错误');  
                    }  
                } else {  
                     ElMessage.error('网络错误，请重试');  
                }  

                store.state.merchant = null;  

                cookie.set('merchant', {});  
            });
            
        } else {  
            ElMessage.error('无效的登录类型');  
            return;  
        }  
    });
}
</script>
 
<template>
    <el-row class="login-page">
        <el-col :span="12" class="bg"></el-col>
        <el-col :span="6" :offset="3" class="form">
            
            <!-- 注册表单 -->
            <el-form ref="refForm" size="large" autocomplete="off" v-if="isRegister"   
                     :model="roleType === 'merchant' ? merchantRegisterData : roleType === 'rider' ? riderRegisterData : userRegisterData"   
                     :rules="roleType === 'merchant' ? merchantRules : roleType === 'rider' ? riderRules : userRules">  
                <el-form-item>  
                    <h1>{{ roleType === 'merchant' ? '商家注册' : roleType === 'rider' ? '骑手注册' : '用户注册' }}</h1>  
                </el-form-item>  
                <div>  
                    <el-radio-group v-model="roleType">  
                        <el-radio label="user">用户注册</el-radio>  
                        <el-radio label="merchant">商家注册</el-radio>  
                        <el-radio label="rider">骑手注册</el-radio>  
                    </el-radio-group>
                    <div>&nbsp;</div>  
                </div>
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
                <el-form-item v-if="roleType === 'merchant'" prop="TimeforOpenBusiness">  
                    <el-time-picker placeholder="请选择营业开始时间" v-model="merchantRegisterData.TimeforOpenBusiness" :picker-options="{ selectableRange: '00:00:00 - 23:59:59' }"></el-time-picker>  
                </el-form-item>  
                <el-form-item v-if="roleType === 'merchant'" prop="TimeForCloseBusiness">  
                    <el-time-picker placeholder="请选择营业结束时间" v-model="merchantRegisterData.TimeforCloseBusiness" :picker-options="{ selectableRange: '00:00:00 - 23:59:59' }"></el-time-picker>  
                </el-form-item>  
                <el-form-item v-if="roleType === 'merchant'" prop="Password">
                    <el-input :prefix-icon="Lock" type="password" placeholder="请输入密码" v-model="merchantRegisterData.Password"></el-input>
                </el-form-item>
                <el-form-item v-if="roleType === 'merchant'" prop="rePassword">
                    <el-input :prefix-icon="Lock" type="password" placeholder="请输入再次密码" v-model="merchantRegisterData.rePassword"></el-input>
                </el-form-item>
                <!--用户注册-->    
                <el-form-item v-if="roleType === 'user'" prop="username">
                    <el-input :prefix-icon="User" placeholder="请输入用户名" v-model="userRegisterData.username"></el-input>
                </el-form-item>
                <el-form-item v-if="roleType === 'user'" prop="phoneNumber">
                    <el-input :prefix-icon="User" placeholder="请输入手机号码" v-model="userRegisterData.phoneNumber"></el-input>
                </el-form-item>
                <el-form-item v-if="roleType === 'user'" prop="password">
                    <el-input :prefix-icon="Lock" type="password" placeholder="请输入密码" v-model="userRegisterData.password"></el-input>
                </el-form-item>
                <el-form-item v-if="roleType === 'user'" prop="rePassword">
                    <el-input :prefix-icon="Lock" type="password" placeholder="请输入再次密码" v-model="userRegisterData.rePassword"></el-input>
                </el-form-item>
                <!--骑手注册-->    
                <el-form-item v-if="roleType === 'rider'" prop="riderName">
                    <el-input :prefix-icon="User" placeholder="请输入用户名" v-model="riderRegisterData.riderName"></el-input>
                </el-form-item>
                <el-form-item v-if="roleType === 'rider'" prop="phoneNumber">
                    <el-input :prefix-icon="User" placeholder="请输入手机号码" v-model="riderRegisterData.phoneNumber"></el-input>
                </el-form-item>
                <el-form-item v-if="roleType === 'rider'" prop="password">
                    <el-input :prefix-icon="Lock" type="password" placeholder="请输入密码" v-model="riderRegisterData.password"></el-input>
                </el-form-item>
                <el-form-item v-if="roleType === 'rider'" prop="rePassword">
                    <el-input :prefix-icon="Lock" type="password" placeholder="请输入再次密码" v-model="riderRegisterData.rePassword"></el-input>
                </el-form-item>
                <!-- 注册按钮 -->
                <el-form-item>
                    <el-button class="button" type="primary" auto-insert-space @click="register">
                        注册
                    </el-button>
                </el-form-item>
                <el-form-item class="flex">
                    <el-link type="info" :underline="false" @click="isRegister = false; clearRegisterData();">
                        ← 返回
                    </el-link>
                </el-form-item>
            </el-form>

            <!-- 登录表单 -->
            <el-form ref="refForm" size="large" autocomplete="off" v-else :model="loginInfo" :rules="loginRules">
                <el-form-item>
                    <h1>登录</h1>
                </el-form-item>
                <el-form-item prop="loginType">  
                    <el-select v-model="loginInfo.loginType" placeholder="选择用户类型">  
                        <el-option label="用户" value="user"></el-option>  
                        <el-option label="商家" value="merchant"></el-option>  
                        <el-option label="骑手" value="rider"></el-option>  
                    </el-select>  
               </el-form-item>
                <el-form-item prop="loginId">
                    <el-input :prefix-icon="User" placeholder="请输入用户Id" v-model="loginInfo.loginId"></el-input>
                </el-form-item>
                <el-form-item prop="loginPassword">
                    <el-input name="password" :prefix-icon="Lock" type="password" placeholder="请输入密码" v-model="loginInfo.loginPassword"></el-input>
                </el-form-item>
                <!-- 登录按钮 -->
                <el-form-item>
                    <el-button class="button" type="primary" auto-insert-space @click="login">登录</el-button>
                </el-form-item>
                <el-form-item class="flex">
                    <el-link type="info" :underline="false" @click="isRegister = true; clearRegisterData();">
                        注册 →
                    </el-link>
                </el-form-item>
            </el-form>
        </el-col>
    </el-row>
</template>
 
<style lang="scss" scoped>
/* 样式 */
.login-page {
    height: 100vh;
    width: 100vw;
    background-color: #fff;
 
    .bg {
        background: url('@/assets/login_bg.jpg') no-repeat center / cover;
        //     
        border-radius: 0 20px 20px 0;
    }
 
    .form {
        display: flex;
        flex-direction: column;
        justify-content: center;
        user-select: none;
 
        .title {
            margin: 0 auto;
        }
 
        .button {
            width: 100%;
        }
 
        .flex {
            width: 100%;
            display: flex;
            justify-content: space-between;
        }
    }
}
</style>