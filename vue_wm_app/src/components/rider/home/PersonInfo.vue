<script setup>
    import { onMounted, ref, provide, inject } from 'vue';
    import imgurl from '@/assets/my_logo.png';
    import { riderInfo, updateRider } from '@/api/rider'
    import { useStore } from "vuex" 
    import { ElMessage } from 'element-plus';
    import { stIdSearch } from "@/api/rider"
    import { getStationsInfo } from "@/api/platform"



    const store = useStore()    
    const refForm = ref(null);
    const rider = ref({})
    const stationId = ref(null); // 用于存储站点 ID  
    const riderStation = ref({}); // 用于存储站点信息  
    const editPIDialogue = ref(false) //编辑个人信息弹窗状态
    const editPassword = ref(false)
    const editWalletPassword = ref(false)
    const currentRiderInfo = inject('providecurrentRiderInfo')
    const riderInformation = inject('provideRiderInformation')
    
    onMounted(() => {  
        // 从 cookie 中读取用户信息  
        const riderData = store.state.rider; 
        if (riderData) {  
            rider.value = riderData;  
        } else {   
            router.push('/login');
        }  
        stIdSearch(rider.value.RiderId).then(res => {  
            stationId.value = res.data; 
            getStationsInfo(stationId.value).then(res => {  
            riderStation.value = res.data; 
            console.log(riderStation);
            }).catch(err => {  
                riderStation.value = "站点信息获取失败"; 
            });  
        }).catch(err => {  
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
        console.log(1);
        const isValid = await refForm.value.validate();  
        if (!isValid) {  
            return; // 如果不合法，提前退出  
        }  
        updateRider(riderInformation.value).then(data=>{
            ElMessage.success('修改成功');
            editPIDialogue.value = false;
            editPassword.value = false;
            editWalletPassword.value = false;
            currentRiderInfo.value.RiderName='';
            currentRiderInfo.value.PhoneNumber='';
            currentRiderInfo.value.Password='';
            currentRiderInfo.value.rePassword='';
            currentRiderInfo.value.WalletPassword='';
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
                <el-descriptions title="骑手个人信息" border>
                    <el-descriptions-item
                    :rowspan="2"
                    :width="140"
                    label="头像"
                    align="center"
                    >
                        <el-avatar
                            class="pic"
                            :src= imgurl
                            alt="wrong"
                            style="width:100px;height: 100px;"
                        />
                    </el-descriptions-item>
                    <el-descriptions-item width="140" align="center" label="ID" >{{riderInformation.RiderId}}</el-descriptions-item>
                    <el-descriptions-item width="140" align="center" label="电话号码">{{riderInformation.PhoneNumber}}</el-descriptions-item>
                    <el-descriptions-item width="140" align="center" label="姓名">{{riderInformation.RiderName}}</el-descriptions-item>
                    <el-descriptions-item width="140" align="center" label="站点">
                        <el-tag size="small">{{ riderStation.stationName }} </el-tag>
                    </el-descriptions-item>
                    <el-descriptions-item align="center" label="地址"> {{ riderStation.stationAddress }}</el-descriptions-item>
                </el-descriptions>


                <el-dropdown class="user-name" trigger="click">
                    <el-button class="editButton" type="primary" circle>
                        <el-icon><Edit /></el-icon>
                    </el-button>
                    <template #dropdown>
                        <el-dropdown-menu>
                            <el-dropdown-item @click="editPIDialogue=true">修改个人信息</el-dropdown-item>
                            <el-dropdown-item divided @click="editPassword=true">修改登录密码</el-dropdown-item>
                            <el-dropdown-item divided @click="editWalletPassword=true">修改钱包密码</el-dropdown-item>
                        </el-dropdown-menu>
                    </template>
                </el-dropdown>
            </div>


    <el-dialog v-model="editPIDialogue" width="500" title="个人信息修改" draggable>
        <el-form label-width="80" ref="refForm" :model="currentRiderInfo" :rules="riderRules">
            <el-form-item label="姓名" prop="RiderName">
                <el-input v-model.lazy="currentRiderInfo.RiderName" placeholder="请更改您的姓名" @blur="validateField('RiderName')"/>
            </el-form-item>
            <el-form-item label="电话号码" prop="PhoneNumber">
                <el-input v-model.lazy="currentRiderInfo.PhoneNumber" placeholder="请更改您的电话" @blur="validateField('PhoneNumber')"/>
            </el-form-item>
            <el-form-item>
                <el-button type="primary" @click="
                    riderInformation.RiderName = currentRiderInfo.RiderName,
                    riderInformation.PhoneNumber = currentRiderInfo.PhoneNumber,
                    submitEdit()">修改</el-button>
                <el-button @click="editPIDialogue=false">取消</el-button>
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
                <el-button type="primary" @click="riderInformation.Password = currentRiderInfo.Password;
                    submitEdit()">修改</el-button>
                <el-button @click="editPassword=false">取消</el-button>
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
                <el-button type="primary" @click="riderInformation.WalletPassword = currentRiderInfo.WalletPassword;
                    submitEdit()">修改</el-button>
                <el-button @click="editWalletPassword=false">取消</el-button>
            </el-form-item>
        </el-form>
    </el-dialog>


</template>

<style scoped>
.personTop {
    display: flex;
    align-items: center;
    margin-bottom: 20px;
    margin-top: 20px;
    margin-left:10px;
    margin-right:10px;
    height: 50%;
    border-radius: 15px; /* 圆角 */
    width:100%
}



.user-name {
    display: flex;
    justify-content: center;
    width: 10%;
}

.editButton {
    height: 50px;
    width: 50px;
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


.pic{
    background-color: #FFF;
}

.el-descriptions {
  background-color: #fff;
  border: 2px solid #ffcc00;
  border-radius: 20px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

</style>