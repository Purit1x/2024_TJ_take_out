<script setup>
import { ElMessage } from 'element-plus';
import { useRouter } from 'vue-router';
import { useStore } from "vuex"  
import { ref, onMounted } from 'vue';
import { submitAddressService, getAddressService, deleteAddressService,EditUserAddress,GetDefaultAddress,CreateDefaultAddress,DeleteDefaultAddress } from '@/api/user';  // 引入API接口
import {cloneDeep} from 'lodash';
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
        if(response.msg='用户地址信息更新成功'){
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
  <div v-if="!isEditing&&!isCreating">
    <h2>我的地址
      <button @click="enterCreate">新建</button>
      <button @click="gobackHome">返回</button>
    </h2>
    <ul>
      <li v-for="address in userAddresses" :key="address.addressId">
        <span>{{ address.contactName }}&nbsp;&nbsp;</span>
        <span>{{ address.phoneNumber }}&nbsp;&nbsp;</span>
        <span>{{ address.userAddress }}&nbsp;&nbsp;</span>
        <span>门牌号：{{ address.houseNumber }}&nbsp;&nbsp;</span>
        <span><button @click="editAddress(address)">编辑</button></span>
        <span><button @click="deleteAddress(address.addressId)">删除</button></span>
        <button v-if="!hasDefaultAddress" @click="setDefaultAddress(address.addressId)">设为默认</button>
        <button v-if="hasDefaultAddress&&DefaultAddressId===address.addressId" @click="deleteDA(address.addressId)">取消默认</button>
    </li>
    </ul>
  </div>
  <div v-if="isEditing|isCreating">
    <h2 v-if="isEditing">编辑地址</h2>
    <h2 v-if="isCreating">新建地址</h2>
    <el-form :rules="addressRules" ref="refForm" :model="currentAddress">
      <el-form-item label="联系人" prop="contactName"><input v-model="currentAddress.contactName" placeholder="联系人" @blur="validateField('contactName')"/></el-form-item>  
      <el-form-item label="电话号码" prop="phoneNumber"><input type="text" v-model="currentAddress.phoneNumber" placeholder="电话号码" @blur="validateField('phoneNumber')"/></el-form-item>  
      <el-form-item label="详细地址" prop="userAddress"><input v-model="currentAddress.userAddress" placeholder="详细地址" @blur="validateField('userAddress')"/></el-form-item>  
      <el-form-item label="门牌号" prop="houseNumber"><input type="text" v-model="currentAddress.houseNumber" placeholder="门牌号" @blur="validateField('houseNumber')"/></el-form-item>  
    </el-form>  
    <button @click="leaveEditing">取消</button>
    <button v-if="isEditing" @click="submitAddress">保存</button>
    <button v-if="isCreating" @click="createAddress">创建</button>
  </div>
</template>