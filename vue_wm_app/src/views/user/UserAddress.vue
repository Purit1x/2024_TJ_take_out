<script setup>
import { ElMessage } from 'element-plus';
import { useRouter } from 'vue-router';
import { useStore } from "vuex"  
import { ref, onMounted } from 'vue';
import { submitAddressService, getAddressService, deleteAddressService } from '@/api/user';  // 引入API接口

const refForm = ref(null);
const addressForm = ref({
    AddressId: 0, 
    UserId: 0, 
    Address: '', 
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
    addressForm.value.AddressId = 0;
    addressForm.value.UserId = 0;
    addressForm.value.Address = '';
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
  <div class="content">
    <header>我的地址</header>

    <!-- 显示已有地址的表格 -->
    <table border="1">
        <thead>
          <tr>
            <th>具体地址</th>
            <th>门牌号</th>
            <th>联系人</th>
            <th>联系电话</th>
            
          </tr>
        </thead>
      <tbody>
        <tr v-for="(address, index) in userAddresses" :key="index">
          <td>{{ address.Address }}</td>
          <td>{{ address.HouseNumber }}</td>
          <td>{{ address.ContactName }}</td>
          <td>{{ address.PhoneNumber }}</td>
          <td>
            <button @click="deleteAddress(address.AddressId)">删除</button>
          </td>
        </tr>
      </tbody>
    </table>

    <form ref="refForm" @submit.prevent="submitAddress">
      <div>
        <label for="address">具体地址</label>
        <input v-model="addressForm.Address" id="address" type="text" placeholder="请输入具体地址" />
      </div>
      <div>
        <label for="houseNumber">门牌号</label>
        <input v-model="addressForm.HouseNumber" id="houseNumber" type="text" placeholder="请输入门牌号" />
      </div>
      <div>
        <label for="contactName">联系人</label>
        <input v-model="addressForm.ContactName" id="contactName" type="text" placeholder="请输入联系人姓名" />
      </div>
      <div>
        <label for="phoneNumber">联系电话</label>
        <input v-model="addressForm.PhoneNumber" id="phoneNumber" type="text" placeholder="请输入联系电话" />
      </div>
      <div>
        <button type="submit">提交</button>
        <button type="button" @click="resetForm">重置</button>
        <button type="button" @click="gobackHome">返回</button>
      </div>
    </form>
  </div>
</template>