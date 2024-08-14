<template>
    <div>
        <div class="person_body">
            <el-descriptions class="margin-top" title="简介" :column="2" border v-if="isOnlyShow">
                <template #extra>
                    <el-button type="primary" size="small" @click="editUser">编辑</el-button>
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
            </el-descriptions>

            <el-form :model="merchantForm" :rules="merchantRules" ref="refForm" label-width="150px" v-else>
                <div class="updateinfo">
                    <el-form-item label="商家名称" prop="MerchantName">
                        <el-input v-model="merchantForm.MerchantName"></el-input>
                    </el-form-item>
                    <el-form-item label="密码" prop="Password">
                        <el-input :prefix-icon="Lock" type="password" placeholder="请输入密码" v-model="merchantForm.Password"></el-input>
                    </el-form-item>
                    <el-form-item label="商家地址" prop="MerchantAddress">
                        <el-input v-model="merchantForm.MerchantAddress"></el-input>
                    </el-form-item>
                    <el-form-item label="联系方式" prop="Contact">
                        <el-input v-model="merchantForm.Contact"></el-input>
                    </el-form-item>
                    <el-form-item label="菜品类型" prop="DishType">
                        <el-input v-model="merchantForm.DishType"></el-input>
                    </el-form-item>
                </div>
                <div class="dialog-footer">
                    <el-button @click="cancelEdit">取 消</el-button>
                    <el-button type="primary" @click="saveUser">提 交</el-button>
                </div>
            </el-form>
        </div>
    </div>
</template>
  
<script setup>
// 导入element-plus的提示组件
import { ElMessage } from 'element-plus'
import { ref, reactive, onMounted, watch } from 'vue';
import { useRouter } from 'vue-router';
import { inject } from 'vue';
import store from '@/store';
const router = useRouter();
const merchant = inject('merchant');
import { userInfo, updateUser } from "@/api/merchant";

const refForm =ref(null);

const merchantForm = ref({
    MerchantId: 0,
    Password:"",
    MerchantName: "",
    MerchantAddress: "",
    Contact: "",
    CouponType: 0,
    DishType: "",
});

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
});

onMounted(() => {  
    merchant.value=store.state.merchant;
    const merchantData = store.state.merchant;

  userInfo(merchantData.MerchantId)
      .then((res) => {
          merchantForm.value.MerchantId = res.data.merchantId;  
          merchantForm.value.MerchantName = res.data.merchantName;  
          merchantForm.value.Password = merchantData.Password;  
          merchantForm.value.MerchantAddress = res.data.merchantAddress;  
          merchantForm.value.Contact = res.data.contact;  
          merchantForm.value.CouponType = res.data.couponType;  
          merchantForm.value.DishType = res.data.dishType;  
      })
      .catch((err) => {
          console.log(err);
      });
});

const goBack = () => {
    router.push('/merchant-home');
    isMerchantHome.value = true;  
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
const isOnlyShow = ref(true);

function editUser() {
    // 除去验证结果
    if(refForm.value)
    {
        refForm.value.clearValidate();
    }

    isOnlyShow.value = false;
}

function saveUser() {
    refForm.value.validate((valid) => {
        if (!valid) {
            return false;
        }
        
        updateUser(merchantForm.value).then(data=>{
            ElMessage.success('修改成功');
            isOnlyShow.value = true;
        })
    });
}

function cancelEdit() {
    isOnlyShow.value = true;
}

function logout() {
    store.dispatch('clearMerchant'); 
    router.push('/login') 
}
</script>

<template>  
  <div>  
    <!-- 左侧导航栏 -->
    <nav class="sidebar">
      <div class="sidebar-content">
        <img class="sidebar-img" src="@\assets\logo.png" alt="logo"/>
        <button class="sidebar-button" @click="goBack">
          <img src="@\assets\merchant_home.png" alt="主页"/>
          <span>主页</span>
        </button>

        <button class="sidebar-button" @click="goToMenu">
          <img src="@\assets\merchant_menu.png" alt="菜单"/>
          <span>本店菜单</span>
        </button>
        
        <button class="sidebar-button" @click="goToPersonal">
          <img src="@\assets\merchant_personal.png" alt="个人信息"/>
          <span>个人信息</span>
        </button>
        <router-view /> <!-- 渲染子路由 -->
      </div>
    </nav>
    <div class="content">
        <h1>这里是个人主页页面，{{merchant.MerchantId}}</h1>  
        <!-- 其他用户信息 --> 
    </div>
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
    width: 1200px;
    position: absolute;
    left: 15%;
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
