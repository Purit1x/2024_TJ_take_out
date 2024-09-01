<script setup>
    import { onMounted, ref } from 'vue';
    import imgurl from '@/assets/logo2.png';
    import { riderInfo, updateRider } from '@/api/rider'
    import { useStore } from "vuex" 
    import { ElMessage } from 'element-plus';

    const store = useStore()    
    const refForm = ref(null);

    const editPIDialogue = ref(false) //编辑个人信息弹窗状态
    const editPassword = ref(false)
    const editWalletPassword = ref(false)
    const currentRiderInfo = ref({})
    const riderInformation = ref({
        RiderId: 0,
        RiderName:'',
        PhoneNumber: '',
        Password: '',
        Wallet: 0,
        WalletPassword: '',
    })

    onMounted(() => {  
        // 从 cookie 中读取用户信息  
        const riderData = store.state.rider; 
        riderInfo(riderData.RiderId)
        .then((res) => {
            riderInformation.value.RiderId=res.data.riderId;
            riderInformation.value.RiderName=res.data.riderName;
            riderInformation.value.PhoneNumber=res.data.phoneNumber;
            riderInformation.value.Password=res.data.password;
            riderInformation.value.Wallet=res.data.wallet;
            riderInformation.value.WalletPassword=res.data.walletPassword;
            currentRiderInfo.value = riderInformation.value;
        })
        .catch((err) => {
            console.log(err);
        });
    });



    const checkRePassword = (rule,value,callback) => {
        if(value == ''){
            callback(new Error('请再次确认密码'))
        } else if(value !==currentRiderInfo.value.Password){
            callback('二次确认密码不相同请重新输入')
        }else{
            callback()
        }
    }
    const checkWalletRePassword = (rule,value,callback) => {
        if(value == ''){
            callback(new Error('请再次确认钱包密码'))
        } else if( value !== currentRiderInfo.value.WalletPassword){
            callback('二次确认钱包密码不相同请重新输入')
        } else{
            callback()
        }
    }
    const riderRules = ref({  
        RiderName: [{ required: true, message: '请输入姓名', trigger: 'blur' }],  
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
    });  

    const validateField = (field) => {  //编辑规则的应用
        refForm.value.validateField(field, (valid) => {  
            if (!valid) {  
                console.log(`验证失败: ${field}`); // 可以根据需要修改  
            }  
        });  
    };  
    const submitEdit = async () => {
        const isValid = await refForm.value.validate();  
        if (!isValid) {  
            return; // 如果不合法，提前退出  
        }  
        updateRider(currentRiderInfo.value).then(data=>{
            ElMessage.success('修改成功');
            editPIDialogue.value = false;
            editPassword.value = false;
            editWalletPassword.value = false;
            riderInformation.value = currentRiderInfo.value;
            currentRiderInfo.value.rePassword='';
            currentRiderInfo.value.reWalletPassword='';
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
</script>

<template>
    <div class="personTop">
            <div class="topLeft">
                <el-avatar
                    size="large"
                    :src= imgurl
                    alt="wrong"
                />
                <div class="name-and-title">
                    <h2>{{riderInformation.RiderName}}</h2>
                    <p>黄金骑手</p>
                </div>
            </div>
            <div class="topRight">
                <el-descriptions title="Personal Information" border>
                    <el-descriptions-item label="Id">{{riderInformation.RiderId}}</el-descriptions-item>
                    <el-descriptions-item label="Name">{{riderInformation.RiderName}}</el-descriptions-item>
                    <el-descriptions-item label="Phone">{{riderInformation.PhoneNumber}}</el-descriptions-item>
                </el-descriptions>
                
                <el-dropdown class="user-name" trigger="click">
                    <span class="changeinfo">
                        <p class="changetext">修改</p>
                        <el-icon>
                            <Operation />
                        </el-icon>
                    </span>
                    <template #dropdown>
                        <el-dropdown-menu>
                            <el-dropdown-item @click="editPIDialogue=true">修改个人信息</el-dropdown-item>
                            <el-dropdown-item divided @click="editPassword=true">修改登录密码</el-dropdown-item>
                            <el-dropdown-item divided @click="editWalletPassword=true">修改钱包密码</el-dropdown-item>
                        </el-dropdown-menu>
                    </template>
                </el-dropdown>
            </div>
    </div>

    <el-dialog v-model="editPIDialogue" width="500" title="个人信息修改" draggable>
        <el-form label-width="80" ref="refForm" :model="currentRiderInfo" :rules="riderRules">
            <el-form-item label="姓名" prop="RiderName">
                <el-input v-model="currentRiderInfo.RiderName" placeholder="请更改您的姓名" @blur="validateField('RiderName')"/>
            </el-form-item>
            <el-form-item label="电话号码" prop="PhoneNumber">
                <el-input v-model="currentRiderInfo.PhoneNumber" placeholder="请更改您的电话" @blur="validateField('PhoneNumber')"/>
            </el-form-item>
            <el-form-item>
                <el-button type="primary" @click="submitEdit">添加</el-button>
                <el-button @click="editPIDialogue=false">重置</el-button>
            </el-form-item>
        </el-form>
    </el-dialog>

    <el-dialog v-model="editPassword" width="500" title="登录密码修改" draggable>
        <el-form label-width="80" ref="refForm" :model="currentRiderInfo" :rules="riderRules">
            <el-form-item label="新密码" prop="Password">
                <el-input v-model="currentRiderInfo.Password" placeholder="请输入登录密码" @blur="validateField('Password')"/>
            </el-form-item>
            <el-form-item label="新密码确认" prop="rePassword">
                <el-input v-model="currentRiderInfo.rePassword"  placeholder="请输入登录密码" @blur="validateField('rePassword')"/>
            </el-form-item>
            <el-form-item>
                <el-button type="primary" @click="submitEdit">添加</el-button>
                <el-button @click="editPassword=false">重置</el-button>
            </el-form-item>
        </el-form>
    </el-dialog>

    <el-dialog v-model="editWalletPassword" width="500" title="钱包密码修改" draggable>
        <el-form label-width="80" ref="refForm" :model="currentRiderInfo" :rules="riderRules">
            <el-form-item label="新密码" prop="WalletPassword">
                <el-input v-model="currentRiderInfo.WalletPassword" placeholder="请输入钱包密码" @blur="validateField('walletPassword')"/>
            </el-form-item>
            <el-form-item label="新密码确认" prop="reWalletPassword">
                <el-input v-model="currentRiderInfo.reWalletPassword"  placeholder="请输入钱包密码" @blur="validateField('reWalletPassword')"/>
            </el-form-item>
            <el-form-item>
                <el-button type="primary" @click="submitEdit">添加</el-button>
                <el-button @click="editWalletPassword=false">重置</el-button>
            </el-form-item>
        </el-form>
    </el-dialog>


</template>

<style scoped>
.personTop {
    display: flex;
    align-items: center;
    justify-content: space-between; 
    background-color: gray;
    margin-bottom: 20px;
    margin-left:10px;
    margin-right:10px;
    height: 20%;
    border-radius: 15px; /* 圆角 */
    width:100%
}

.topLeft {
    display: flex;
    align-items: center; 
    margin-left:10px;
}

.topRight {
    display: flex;
    align-items: center;
    margin-right: 20px;

}

.name-and-title {
    margin-left: 30px;
}
.user-avator {
    display: flex;
    justify-content: center;
    align-items: center;
    margin-left:15px;
}

.changeinfo {
    display: flex;
    justify-content: center;
    align-items: center;
    margin-left: 10px;
    background-color: aqua;
    border: none;
    border-radius: 12px;
    color:black;
    width: 100px;
}

.changetext {
    user-select: none;
}

</style>