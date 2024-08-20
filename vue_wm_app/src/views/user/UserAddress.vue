<script setup>
import { ElMessage } from 'element-plus';
import { useRouter } from 'vue-router';
import { useStore } from "vuex"  
import { ref, onMounted } from 'vue';
import { submitAddressService, getAddressService, deleteAddressService } from '@/api/user';  // 引入API接口

const refForm = ref(null);
const addressForm = ref({
    UserId: 0, 
    Address: '', 
    AddressId: 0, 
    HouseNumber: '', 
    ContactName: '', 
    PhoneNumber: '',
});
const router = useRouter();
const store = useStore();
const userAddresses = ref([]);

// 提交地址表单函数
const submitAddress = async () => {
    const isValid = await refForm.value.validate();
    if (!isValid) return;
    try {
      await submitAddressService(addressForm.value);
      alert('提交成功');
      resetForm();
      onMounted();//重新加载地址以刷新表格
    } catch (error) {
        console.error('提交地址信息失败:', error);
    }
};

// 重置表单函数
const resetForm = () => {
    addressForm.value.UserId = 0;
    addressForm.value.Address = '';
    addressForm.value.AddressId = 0;
    addressForm.value.HouseNumber = '';
    addressForm.value.ContactName = '';
    addressForm.value.PhoneNumber = '';
};

// 返回主页函数
const gobackHome = () => {
  router.push('/user-home');
};

// 初始化页面
onMounted(async () => {
  try{
    const userId = store.state.user.userId; 
    const userAddress = await getAddressService(userId);
    userAddresses.value = [userAddress]; // 假设返回的是一个地址对象，放入数组中以便显示在表格里

    addressForm.value = {
      Address: userAddress.address || '',
      HouseNumber: userAddress.houseNumber || '',
      ContactName: userAddress.contactName || '',
      PhoneNumber: userAddress.phoneNumber || '',
      AddressId: userAddress.addressId || 0,
      UserId: userAddress.userId || 0,
    };
    console.log('addressForm.value:', addressForm.value);
  } catch (error) {
    console.error('加载用户地址信息失败：',error);
  }
});

// 删除地址函数
const deleteAddress = async (addressId) => {
    try {
        await deleteAddressService(addressId);
        alert('删除成功');
        userAddresses.value = userAddresses.value.filter(address => address.id !== addressId);
    } catch (error) {
        console.error('删除地址失败:', error);
    }
};
</script>

<template>
  <div>
    <div>地址页面&nbsp;&nbsp;</div>
    <h1>地址录入</h1>

    <!-- 显示已有地址的表格 -->
    <el-table :data="userAddresses" border style="width: 100%">
      <el-table-column prop="Address" label="具体地址" width="180"></el-table-column>
      <el-table-column prop="HouseNumber" label="门牌号" width="180"></el-table-column>
      <el-table-column prop="ContactName" label="联系人" width="180"></el-table-column>
      <el-table-column prop="PhoneNumber" label="联系电话" width="180"></el-table-column>
      <el-table-column label="操作" width="100">
        <template v-slot="scope">
          <el-button @click="deleteAddress(scope.row.AddressId)" type="danger" size="small">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-form ref="refForm" @submit.prevent="submitAddress" :model="addressForm" label-width="100px">
      <el-form-item label="具体地址">
        <el-input v-model="addressForm.Address" placeholder="请输入具体地址"></el-input>
      </el-form-item>
      <el-form-item label="门牌号">
        <el-input v-model="addressForm.HouseNumber" placeholder="请输入门牌号"></el-input>
      </el-form-item>
      <el-form-item label="联系人">
        <el-input v-model="addressForm.ContactName" placeholder="请输入联系人姓名"></el-input>
      </el-form-item>
      <el-form-item label="联系电话">
        <el-input v-model="addressForm.PhoneNumber" placeholder="请输入联系电话"></el-input>
      </el-form-item>
      <el-form-item>
        <el-button type="primary" @click="submitAddress">提交</el-button>
        <el-button @click="resetForm">重置</el-button>
        <el-button @click="gobackHome">返回</el-button>
      </el-form-item>
    </el-form>
  </div>
</template>