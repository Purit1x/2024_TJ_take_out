<script setup>
    import { useRouter } from 'vue-router';
    import { useStore } from "vuex" 
    import { ref,onMounted, provide} from 'vue'
    import { riderInfo, updateRider,walletWithdraw } from '@/api/rider'
    import { ElMessage } from 'element-plus';
    import PersonInfo from '@/components/rider/home/PersonInfo.vue';

    const store = useStore()    

    const refForm = ref(null);

    const isWithdraw = ref(false)
    const riderInformation = ref({
        RiderId: 0,
        RiderName:'',
        PhoneNumber: '',
        Password: '',
        Wallet: 0,
        WalletPassword: '',
    })

    const currentRiderInfo = ref({
        RiderId: 0,
        RiderName:'',
        PhoneNumber: '',
        Password: '',
        Wallet: 0,
        WalletPassword: '',
        rePassword:'',
        reWalletPassword:'',
        withdrawAmount: 0
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
            currentRiderInfo.value.RiderId = riderInformation.value.RiderId;
            currentRiderInfo.value.Wallet = riderInformation.value.Wallet;
        })
        .catch((err) => {
            console.log(err);
        });
    });


    provide("provideRiderInformation",riderInformation);
    provide("providecurrentRiderInfo",currentRiderInfo);

    const withdraw = async() => {
        const isValid = await refForm.value.validate();   
        if (!isValid) return; // 如果不合法，提前退出
        if(currentRiderInfo.value.Wallet < currentRiderInfo.value.withdrawAmount) 
        {
            ElMessage.error('提现金额超出钱包金额')
            return
        }
        console.log(currentRiderInfo.value.WalletPassword)
        console.log(riderInformation.value.WalletPassword)
        if(currentRiderInfo.value.WalletPassword != riderInformation.value.WalletPassword)
        {
            ElMessage.error('密码错误')
            return
        }
        walletWithdraw(currentRiderInfo.value.RiderId,currentRiderInfo.value.withdrawAmount).then(data=>{
            currentRiderInfo.value.Wallet=data.data;
            riderInformation.value.Wallet=data.data;
            ElMessage.success('提现成功');
            isWithdraw.value=false;
            currentRiderInfo.value.WalletPassword=''
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
        WalletPassword:[
            {required:true, message:'请输入支付密码', trigger:'blur'},
            {  
                pattern: /^[0-9]{6}$/, 
                message: '支付密码必须是6位数字', // 错误提示消息  
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

</script>

<template>
    <div class="main_body">
        <div class="person_body">
            <PersonInfo />
            <div class="personDown">
                <div class="personside">
                    <div class="headline">
                        工资钱包<el-icon class="el-icon--right"><Wallet /></el-icon>
                    </div>
                    <div class="moneytake">
                        <div class="moneyinfo">
                            ￥&nbsp;{{ riderInformation.Wallet}}
                        </div>
                        <el-button type="primary" @click="isWithdraw=true">
                            提现
                        </el-button>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <el-dialog v-model="isWithdraw" width="500" title="提现页面" draggable>
        <el-form label-width="80" ref="refForm" :model="currentRiderInfo" :rules="riderRules">
            <el-form-item label="提现金额" prop="withdrawAmount">
                <el-input v-model.lazy="currentRiderInfo.withdrawAmount" placeholder="请更改您的姓名" @blur="validateField('withdrawAmount')"/>
            </el-form-item>
            <el-form-item label="密码" prop="WalletPassword">
                <el-input v-model.lazy="currentRiderInfo.WalletPassword" placeholder="请输入您的密码" @blur="validateField('WalletPassword')"/>
            </el-form-item>
            <el-form-item>
                <el-button type="primary" @click="withdraw">修改</el-button>
                <el-button @click="isWithdraw=false">取消</el-button>
            </el-form-item>
        </el-form>
    </el-dialog>
</template>

<style scoped>
.main_body {
    display:flex;
    justify-content: center;
    align-items: center;
    height: 100%;
    width: 100%;
}

.person_body {
    width: 70%;
    height: 70%;
    padding: 20px;
    background-color: #ffd666;
    border: 2px solid #000000;
    border-radius: 20px;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
}


.moneytake {
    display: flex;
    justify-content: space-around;
    align-items: center;
    height: 60%;
}
.moneyinfo {
    font-size: large;
}

.personDown {
    display:flex;
    color:black;
    height: 30%;
}

.personside {
    width:100%;
    height: 100%;
    text-align: center; 
    margin-left: 10px;
    margin-right: 10px;    
    background-color: #ffffff;
    border: 2px solid #ffcc00;
    border-radius: 20px;
    padding: 20px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}
.personInfo {
    flex: 2; 
    text-align: center; 
    margin-left: 10px;
    margin-right: 10px;    
    padding: 20px; 
    border-radius: 15px; 
    box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1); 

}

.headline {
    font-size: 20px;
    display: flex;
    justify-content: center;
    align-items: center;
}



</style>    