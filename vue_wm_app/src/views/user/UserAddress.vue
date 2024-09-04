<script setup>
import { ElMessage } from 'element-plus';
import { useRouter } from 'vue-router';
import { useStore } from "vuex"  
import { ref, onMounted } from 'vue';
import { submitAddressService, getAddressService, deleteAddressService,EditUserAddress,GetDefaultAddress,CreateDefaultAddress,DeleteDefaultAddress } from '@/api/user';  // 引入API接口
import {cloneDeep} from 'lodash';

import {
  House,
  Edit,
  Delete,
  FolderAdd,
  Star,
  StarFilled,
} from '@element-plus/icons-vue'
const refForm = ref(null);
const router = useRouter();
const store = useStore();
const user = ref({}); // 初始化用户信息对象
const userAddresses = ref([]);
const currentAddress = ref({}); // 用于跟踪当前正在编辑的地址对象
const userId = ref(null); // 用于跟踪用户ID
const isEditing = ref(false); // 用于跟踪是否处于编辑模式
const isCreating = ref(false); // 用于跟踪是否处于新建模式
const hasDefaultAddress = ref(true); // 用于跟踪是否有默认地址
const DefaultAddressId=ref(null); // 用于跟踪默认地址ID
// 初始化页面
onMounted(async () => {
  try{
    const userData = store.state.user;
    if (userData) {
      user.value = userData;
    } else {
      router.push('/login');
    }
    const userid = store.state.user.userId; 
    userId.value = userid;
    userAddresses.value = await getAddressService(userid);
    const response = await GetDefaultAddress(userid);
    if(response.data==='none')
    {
      hasDefaultAddress.value=false;
    }
    else
    {
      DefaultAddressId.value=response.data;
      console.log('DefaultAddressId:',DefaultAddressId.value);
    }
    console.log('userAddress.value:', userAddresses);
  } catch (error) {
    console.error('加载用户地址信息失败：',error);
  }
});
const gobackHome = () => {
  router.push('/user-home');
}
const editAddress = (address) => {
  currentAddress.value = cloneDeep(address);
  isEditing.value = true;
}
const addressRules = ref({  
    contactName: [{ required: true, message: '请输入用户名', trigger: 'blur' }],  
    phoneNumber: [
        { required: true, message: '请输入电话号码', trigger: 'blur' },
        {  
            pattern: /^[0-9]{11}$/, // 确保输入是长度为11的数字  
            message: '电话号码必须是11位数字', // 错误提示消息  
            trigger: 'blur', 
        },  
    ],
    userAddress: [{ required: true, message: '请输入详细地址', trigger: 'blur' }], 
    houseNumber: [{ required: true, message: '请输入门牌号', trigger: 'blur' }],
});
const validateField = (field) => {  //编辑规则的应用
    refForm.value.validateField(field, (valid) => {  
        if (!valid) {  
            console.log(`验证失败: ${field}`); // 可以根据需要修改  
        }  
    });  
};  
const submitAddress = async() => {  // 保存地址信息
  const isValid = await refForm.value.validate(); // 使用 dishForm 引用进行验证  
  if (!isValid) return; // 如果不合法，提前退出  
  console.log('currentAddress:',currentAddress.value);
    const data={
        UserId:currentAddress.value.userId,
        AddressId:currentAddress.value.addressId,
        ContactName:currentAddress.value.contactName,
        PhoneNumber:currentAddress.value.phoneNumber,
        Address:currentAddress.value.userAddress,
        HouseNumber:currentAddress.value.houseNumber,
    }
    try {  
        const response = await EditUserAddress(data);  
        if(response.msg==='用户地址信息更新成功'){
            ElMessage.success('更新成功');
            isEditing.value = false;
            currentAddress.value = {};
            userAddresses.value = await getAddressService(userId.value);
        } else {
            ElMessage.error('尚未更改信息');
        }
    } catch (error) {  
        ElMessage.error('尚未更改信息');
    }  
}
const leaveEditing = () => {
  isEditing.value = false;
  isCreating.value = false;
  currentAddress.value = {};
}
const enterCreate = ()=> {
  currentAddress.value = {
      contactName: '',
      phoneNumber: '',
      userAddress: '',
      houseNumber: '',
      userId: userId.value,
      addressId: '',
  };
  isCreating.value = true;
  isEditing.value = false;
}
const createAddress = async() => {  // 新建地址信息
  const isValid = await refForm.value.validate(); // 使用 dishForm 引用进行验证  
  if (!isValid) return; // 如果不合法，提前退出  
    const data={
        UserId:userId.value,
        ContactName:currentAddress.value.contactName,
        PhoneNumber:currentAddress.value.phoneNumber,
        Address:currentAddress.value.userAddress,
        HouseNumber:currentAddress.value.houseNumber,
        AddressId:0,
    }
    try {  
        const response = await submitAddressService(data);  
        if(response.msg='用户地址信息创建成功'){
            ElMessage.success('创建成功');
            if(userAddresses.length==1)hasDefaultAddress.value=false;
            isCreating.value = false;
            isEditing.value = false;
            currentAddress.value = {};
            userAddresses.value = await getAddressService(userId.value);
        } else {
            ElMessage.error('尚未创建信息');
        }
    } catch (error) {  
        console.error('创建失败：',error);  
    }  
}

const deleteAddress = async(addressId) => {  // 删除地址信息
  try {  
    const response = await deleteAddressService(addressId);  
        ElMessage.success('删除成功');
        userAddresses.value = await getAddressService(userId.value);
  } catch (error) {  
    userAddresses.value=[];
  }  
}
const deleteDA = async(addressId) => {  // 删除默认地址信息
  try {  
    const response = await DeleteDefaultAddress(addressId);  
    if(response.msg='默认地址设置删除成功'){
        ElMessage.success('取消成功');
        hasDefaultAddress.value=false;
        DefaultAddressId.value=null;
    } else {
        ElMessage.error('删除失败');
    }
  } catch (error) {  
    ElMessage.error('删除失败');
  }  
}
const setDefaultAddress = async(addressId) => {  // 设置默认地址信息
  try {  
    const data={
        UserId:userId.value,
        AddressId:addressId,
    }
    const response = await CreateDefaultAddress(data);  
    if(response.data=200){
        ElMessage.success('设置成功');
        hasDefaultAddress.value=true;
        DefaultAddressId.value=addressId;
    } else {
        ElMessage.error('设置失败');
    }
  } catch (error) {  
    ElMessage.error('设置失败');
  }  
}
</script>

<template>
  <div v-if="!isEditing&&!isCreating" class="content">
    <h2 class="header">我的地址</h2>

    <table class="address-table">
      <thead>
        <tr>
          <th>联系人</th>
          <th>电话号码</th>
          <th>详细地址</th>
          <th>门牌号</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="address in userAddresses" :key="address.addressId">
          <td>{{ address.contactName }}</td>
          <td>{{ address.phoneNumber }}</td>
          <td>{{ address.userAddress }}</td>
          <td>{{ address.houseNumber }}</td>
          <td>
            <el-button-group class="editbutton">
              <el-tooltip content="编辑" hide-after="0" placement="bottom">
                <el-button class="primary-button" @click="editAddress(address)" :icon="Edit" />
              </el-tooltip>
              <el-tooltip content="删除" hide-after="0" placement="bottom">
                <el-button class="primary-button" @click="deleteAddress(address.addressId)" :icon="Delete" />
              </el-tooltip>
              <el-tooltip content="设为默认" hide-after="0" placement="bottom">
                <el-button class="primary-button" v-if="!hasDefaultAddress" @click="setDefaultAddress(address.addressId)" :icon="Star" />
              </el-tooltip>
              <el-tooltip content="取消默认" hide-after="0" placement="bottom">
                <el-button class="primary-button" v-if="hasDefaultAddress&&DefaultAddressId===address.addressId" @click="deleteDA(address.addressId)" :icon="StarFilled" />
              </el-tooltip>
            </el-button-group>
          </td>
        </tr>
        <tr>
          <td colspan="5" class="new-address-button-container">
            <el-tooltip content="新建地址" hide-after="0" placement="top">
              <el-button class="primary-button" @click="enterCreate" :icon="FolderAdd">
                新建地址
              </el-button>
            </el-tooltip>
          </td>
        </tr>
      </tbody>
    </table>
  </div>

  <div v-if="isEditing||isCreating" class="edit-container">
    <h2 v-if="isEditing">编辑地址</h2>
    <h2 v-if="isCreating">新建地址</h2>
    <el-form :rules="addressRules" ref="refForm" :model="currentAddress">
      <el-form-item label="联系人" prop="contactName">
        <input v-model="currentAddress.contactName" placeholder="联系人" @blur="validateField('contactName')" />
      </el-form-item>  
      <el-form-item label="电话号码" prop="phoneNumber">
        <input type="text" v-model="currentAddress.phoneNumber" placeholder="电话号码" @blur="validateField('phoneNumber')" />
      </el-form-item>  
      <el-form-item label="详细地址" prop="userAddress">
        <input v-model="currentAddress.userAddress" placeholder="详细地址" @blur="validateField('userAddress')" />
      </el-form-item>  
      <el-form-item label="门牌号" prop="houseNumber">
        <input type="text" v-model="currentAddress.houseNumber" placeholder="门牌号" @blur="validateField('houseNumber')" />
      </el-form-item>  
    </el-form>  
    <div class="button-group">
      <el-button class="secondary-button" @click="leaveEditing">取消</el-button>
      <el-button class="primary-button" v-if="isEditing" @click="submitAddress">保存</el-button>
      <el-button class="primary-button" v-if="isCreating" @click="createAddress">创建</el-button>
    </div>
  </div>
</template>

<style scoped>
/* 页面整体样式 */
.content {
  padding: 20px;
  background-color: transparent;
  font-family: Arial, sans-serif;
}

.header {
  font-size: 28px;
  font-weight: bold;
  color: #333;
  margin-bottom: 20px;
}

/* 表格样式 */
.address-table {
  width: 100%;
  border-collapse: collapse;
  margin-bottom: 20px;
  font-size: 16px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.address-table th {
  background-color: #7BA7AB;
  color: white;
  padding: 12px;
}

.address-table td {
  padding: 12px;
  text-align: center;
  border-bottom: 1px solid #ddd;
}

.address-table tbody tr:nth-of-type(even) {
  background-color: #f3f3f3;
}

.new-address-button-container {
  text-align: center;
  padding-top: 20px;
}

/* 按钮组样式 */
.primary-button {
  background-color: #ff69b4;
  border: none;
  color: white;
  border-radius: 20px;
  padding: 6px 12px;
  cursor: pointer;
  transition: background-color 0.3s, box-shadow 0.3s;
}

.primary-button:hover {
  background-color: #ff85c1;
  box-shadow: 0 0 8px rgba(255, 105, 180, 0.8);
}

.secondary-button {
  background-color: #ccc;
  border: none;
  color: white;
  border-radius: 20px;
  padding: 6px 12px;
  cursor: pointer;
  transition: background-color 0.3s, box-shadow 0.3s;
}

.secondary-button:hover {
  background-color: #ddd;
  box-shadow: 0 0 8px rgba(204, 204, 204, 0.8);
}

.button-group {
  margin-top: 20px;
  display: flex;
  justify-content: flex-start;
  gap: 10px;
}

.edit-container {
  width: calc(1000% - 50px); /* 调整表格宽度，考虑侧边栏的宽度 */
  margin: 0 auto;  /* 水平居中 */
  margin-left: 600px;
  margin-top: -150px;
  padding: 20px;
  background-color: #f9f9f9;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);  /* 使用 transform 将其垂直和水平居中 */
}

.el-form-item {
  margin-bottom: 20px;
}

.el-form-item input {
  width: 100%;
  padding: 10px;
  border: 1px solid #ccc;
  border-radius: 4px;
  font-size: 16px;
  box-sizing: border-box;
}

.button-group {
  display: flex;
  justify-content: flex-end;  /* 按钮组右对齐 */
  gap: 10px;
}

</style>
