<script setup>
// 导入element-plus的提示组件
import { ElMessage } from 'element-plus'
// 导入用户图标和锁图标
import { User, Lock } from '@element-plus/icons-vue'
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

//控制注册与登录表单的显示， 默认显示注册
const isRegister = ref(false)
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
    TimeForOpenBussiness:null,
    TimeForCloseBussiness:null,
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
        TimeForOpenBussiness:null,
        TimeForCloseBussiness:null,
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
    } else if( value !== userRegisterData.value.password){
        callback('二次确认密码不相同请重新输入')
    } else{
        callback()
    }
}
 
//定义表单校验规则
const rules = ref({
    username:[
        {required:true, message:'请输入用户名', trigger:'blur'},
        {min:2, max:16, message:'请输入长度5~16非空字符', trigger:'blur'}
    ],
    phoneNumber:[
        {required:true, message:'请输入手机号码', trigger:'blur'},
    ],
    password:[
        {required:true, message:'请输入密码', trigger:'blur'},
        {min:3, max:16, message:'请输入长度5~16非空字符', trigger:'blur'}
    ],
    rePassword:[{validator:checkRePassword,trigger:'blur'}] //校验二次输入密码是否相同
})
const loginRules = ref({
    loginType:[
        {required:true, message:'请选择用户类型', trigger:'blur'},
    ],
    loginId:[
        {required:true, message:'请输入用户Id', trigger:'blur'},
    ],
    loginPassword:[
        {required:true, message:'请输入密码', trigger:'blur'},
        {min:3, max:16, message:'请输入长度5~16非空字符', trigger:'blur'}
    ]
})
//调用后台接口完成注册
import {userRegisterService, userLoginService} from '@/api/user.js'
import {merchantRegisterService, merchantLoginService} from '@/api/merchant.js'
const register = ()=> {
     // 先验证数据
     refForm.value.validate((valid) => {
        if (!valid) {
            return false;
        }

       // 调用注册接口  
       userRegisterService({  
            UserName: userRegisterData.value.username, // 后端要求的字段  
            Password: userRegisterData.value.password,  
            PhoneNumber: userRegisterData.value.phoneNumber  
        }).then(res => {  
            // 成功  
            ElMessage.success({  
                message: '注册成功，用户ID为 ' + res.data + '，请牢记该Id。',  
                duration: 10000 // 设置显示时间为10秒  
            });  
            clearRegisterData() // 注册成功后清空输入  
            // 可选择跳转到其他页面，例如登录 page  
            // router.push('/login')  
        }).catch(err => {  
            // 处理错误  
            ElMessage.error(`注册失败: ${err.response.data.msg || '未知错误'}`)  
        })  
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
                    store.state.user = userRegisterData.value;
                    // 保持cookie
                    cookie.set('user', userRegisterData.value);
                    // 跳转
                    router.push('/');
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
                store.state.merchant = {};  
                cookie.set('merchant', {});  
            });
        } else if (loginTypeValue === 'merchant') {  //商家登录
            //调用接口完成登录
            merchantRegisterData.value.MerchantId = loginInfo.value.loginId;
            merchantRegisterData.value.Password = loginInfo.value.loginPassword;
            merchantLoginService(merchantRegisterData.value).then(data => {
                if(data.msg=== "ok"){
                    ElMessage.success('登录成功');
                    store.state.merchant = merchantRegisterData.value;
                    cookie.set('merchant', merchantRegisterData.value);
                    router.push('/');
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

                store.state.merchant = {};  
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
            <el-form ref="refForm" size="large" autocomplete="off" v-if="isRegister" :model="userRegisterData" :rules="rules">
                <el-form-item>
                    <h1>注册</h1>
                </el-form-item>
                <el-form-item prop="username">
                    <el-input :prefix-icon="User" placeholder="请输入用户名" v-model="userRegisterData.username"></el-input>
                </el-form-item>
                <el-form-item prop="phoneNumber">
                    <el-input :prefix-icon="User" placeholder="请输入手机号码" v-model="userRegisterData.phoneNumber"></el-input>
                </el-form-item>
                <el-form-item prop="password">
                    <el-input :prefix-icon="Lock" type="password" placeholder="请输入密码" v-model="userRegisterData.password"></el-input>
                </el-form-item>
                <el-form-item prop="rePassword">
                    <el-input :prefix-icon="Lock" type="password" placeholder="请输入再次密码" v-model="userRegisterData.rePassword"></el-input>
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