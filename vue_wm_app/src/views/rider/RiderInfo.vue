<script setup>
    import { useRouter } from 'vue-router';
    import { useStore } from "vuex" 
    import { ref,onMounted, provide} from 'vue'
    import { riderInfo, updateRider,walletRecharge } from '@/api/rider'
    import { ElMessage } from 'element-plus';
    import PersonInfo from '@/components/rider/home/PersonInfo.vue';

    const refForm =ref(null);
    const router = useRouter()
    const store = useStore()    
    const currentRiderInfo = ref({})
    const personalInfo = ref(false);  // 个人信息弹窗状态
    // const editPI=ref(false)  //编辑个人信息弹窗状态
    const editPIDialogue = ref(false) //编辑个人信息弹窗状态
    const isWallet=ref(false);  //是否是钱包界面
    const isRecharge=ref(false);  //是否是充值界面
    const isChangeWP=ref(false);  //是否是修改支付密码界面
    const riderForm = ref({
        RiderId: 0,
        RiderName:'',
        PhoneNumber: '',
        Password: '',
        rePassword:'',  
        Wallet: 0,
        WalletPassword: '',
        reWalletPassword:'',
        recharge:0,
    })


    onMounted(() => {  
        // 从 cookie 中读取用户信息  
        const riderData = store.state.rider; 
        riderInfo(riderData.RiderId)
        .then((res) => {
            riderForm.value.RiderId=res.data.riderId;
            riderForm.value.RiderName=res.data.riderName;
            riderForm.value.PhoneNumber=res.data.phoneNumber;
            riderForm.value.Password=res.data.password;
            riderForm.value.Wallet=res.data.wallet;
            riderForm.value.WalletPassword=res.data.walletPassword;
            currentRiderInfo.value = riderForm.value;
        })
        .catch((err) => {
            console.log(err);
        });
    });

    provide("provideRiderInfo",riderForm);

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
    const submitEdit = async () => {
        const isValid = await refForm.value.validate();  
        if (!isValid) {  
            return; // 如果不合法，提前退出  
        }  
        updateRider(currentRiderInfo.value).then(data=>{
            ElMessage.success('修改成功');
            editPI.value = false;
            personalInfo.value = true;
            riderForm.value = currentRiderInfo.value;
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
        updateRider(currentRiderInfo.value).then(data=>{
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

</script>

<template>
    骑手信息页
    <PersonInfo />
    <div class="personDown">
        <div class="personside">
            工资钱包
        </div>
        <div class="personInfo">

        </div>
    </div>
</template>

<style scoped>


.personDown {
    display:flex;
    color:white;
    height: 65%;
}

.personside {
    flex: 1; 
    text-align: center; 
    margin-left: 10px;
    margin-right: 10px;    
    background-color: gray;
    padding: 20px; 
    border-radius: 15px; 
    box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1); 
}
.personInfo {
    flex: 2; 
    text-align: center; 
    margin-left: 10px;
    margin-right: 10px;    
    background-color: gray;
    padding: 20px; 
    border-radius: 15px; 
    box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1); 

}

</style>    

<!-- <div v-if="!personalInfo && !isWallet&&!editPI&&!isRecharge&&!isChangeWP">
        <div><button @click="enterPersonalInfo">个人信息</button></div>
        <div><button @click="enterWallet">钱包</button></div>
    </div>
    <div v-if="personalInfo">
        <div>骑手Id：{{currentRiderInfo.RiderId}}</div>
        <div>姓名：{{currentRiderInfo.RiderName}}</div>
        <div>手机号：{{currentRiderInfo.PhoneNumber}}</div>
        <div>密码：{{currentRiderInfo.Password}}</div>
        <div>
            <button @click="editPersonalInfo">编辑个人信息</button>
            <button @click="leavePersonalInfo">&nbsp;返回</button>
        </div>
    </div>
    <div v-if="editPI">
        <el-form ref="refForm" :model="currentRiderInfo" :rules="riderRules">
                <el-form-item label="用户名" prop="UserName"><input v-model="currentRiderInfo.RiderName" placeholder="姓名" @blur="validateField('RiderName')"/></el-form-item>
                <el-form-item label="手机号" prop="PhoneNumber"><input type="number" v-model="currentRiderInfo.PhoneNumber" placeholder="手机号" @blur="validateField('PhoneNumber')"/></el-form-item>
                <el-form-item label="密码" prop="Password"><input type="password" v-model="currentRiderInfo.Password" placeholder="密码" @blur="validateField('Password')"/></el-form-item>
                <el-form-item label="确认密码" prop="rePassword"><input type="password" v-model="currentRiderInfo.rePassword" placeholder="确认密码" @blur="validateField('rePassword')"/></el-form-item>
            </el-form>
            <button @click="submitEdit()">提交</button>
            <button @click="leaveEdit()">取消</button>
    </div>
    <div v-if="isWallet">
        <div>钱包金额：{{riderForm.Wallet}}</div>
        <button @click="OpenRechargeWindow">充值</button>
        <button @click="OpenWPWindow">修改支付密码</button>
        <button @click="leaveWallet">返回</button>
    </div>
    <el-form :model="currentRiderInfo" :rules="riderRules" ref="refForm">
   
        <div class="recharge" v-if="isRecharge">  
            <div>充值金额</div>
            <el-form-item label="充值金额" prop="recharge"><input type="number" v-model="currentRiderInfo.recharge" placeholder="请输入充值金额" @blur="validateField('recharge')"/></el-form-item>
            <button @click="SaveRecharge">充值</button>
            <button>提现</button>
            <button @click="leaveRechargeWindow">返回</button>
        </div>
    
        <div class="changewp" v-if="isChangeWP"> 
            <div>支付密码</div>
            <el-form-item label="支付密码" prop="WalletPassword"><input type="password" v-model="currentRiderInfo.WalletPassword" placeholder="请输入支付密码" @blur="validateField('WalletPassword')"/></el-form-item>
            <div>确认支付密码</div>
            <el-form-item labal="确认支付密码" prop="reWalletPassword"><input type="password" v-model="currentRiderInfo.reWalletPassword" placeholder="请再次确认支付密码" @blur="validateField('reWalletPassword')"/></el-form-item>
            <button @click="SaveWalletPassword">修改</button>
            <button @click="leaveWPWindow">返回</button>
        </div>
    </el-form> -->