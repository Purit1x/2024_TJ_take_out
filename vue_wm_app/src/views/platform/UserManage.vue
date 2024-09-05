

<script setup>
import { ref, onMounted, watch } from 'vue';
import { ElMessage, ElMessageBox } from "element-plus";
import { getAllUsers, deleteUser } from "@/api/user"; // 确保正确导入 getAllUsers 和 deleteUser
import { useRouter } from 'vue-router';
const router = useRouter();
const users = ref([]); // 存储所有用户信息
const showUsers = ref([]); // 显示用户信息列表
const searchQuery = ref(''); // 搜索框内容

// 获取用户列表
onMounted(async () => {
  await fetchUsers();
});

const fetchUsers = async () => {
  try {
    const response = await getAllUsers();
    if (response!=null) {
      users.value = response;
      showUsers.value = response;
      
    }
  } catch (error) {
    ElMessage.error('获取用户列表失败');
  }
  console.log('showData',showUsers.value);
};

// 搜索用户
const handleSearch = () => {
  const query = searchQuery.value.trim().toLowerCase();
  
  if (query) {
    if (Array.isArray(users.value)) { // 检查 users.value 是否是一个数组
      showUsers.value = users.value.filter(user => 
        user.userName.toLowerCase().includes(query) ||
        user.phoneNumber.includes(query)
        
      );
    } else {
      console.error('users.value is not an array, cannot perform filter operation.');
      showUsers.value = []; // 设置为空数组以避免显示无效数据
    }
  } else {
    if (Array.isArray(users.value)) { // 同样检查 users.value 是否是一个数组
      showUsers.value = users.value;
    } else {
      showUsers.value = []; // 同样设置为空数组
    }
  }


};



// 格式化钱包余额显示
const formatWallet = (row, column, cellValue, index) => {
  return `¥ ${cellValue.toFixed(2)}`;
};

// 监听 searchQuery 的变化，以便在用户输入时自动搜索
watch(searchQuery, () => {
  handleSearch();
});
const gobackHome = () => {
    router.push('/platform-home');
}
</script>


<template>
  <div class="user-manage">
    <div class="head">用户列表</div>
    <div class="top">
      <input type="text" v-model="searchQuery" placeholder="搜索用户名或电话号码" @keyup.enter="handleSearch" class="inputtext" />
      <button @click="handleSearch" class="search">搜索</button>
    </div>

    <el-table :data="showUsers" :border="true" style="width: 100%" class="table">
      <el-table-column prop="userId" label="用户ID" width="80"/>
      <el-table-column prop="userName" label="用户名" />
      <el-table-column prop="phoneNumber" label="电话号码" />
      <el-table-column prop="wallet" label="钱包余额" :formatter="formatWallet" />
      
        
      
    </el-table>
    <button @click="gobackHome" class="return">返回</button>
  </div>
</template>


<style scoped>
.user-manage {
  padding: 20px;
  background-color: #f0f0f0;
  border-radius: 8px;
}

.head {
  font-size: 24px;
  margin-bottom: 20px;
}

.top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.inputtext {
  width: 300px;
  height: 40px;
  padding: 0 10px;
  border: 1px solid #ccc;
  border-radius: 4px;
}

.search {
  height: 40px;
  padding: 0 20px;
  background-color: #409eff;
  border: none;
  border-radius: 4px;
  color: white;
  cursor: pointer;
}

.search:hover {
  background-color: #337ecc;
}

.table {
  width: 100%;
  border: 1px solid #ccc;
  border-radius: 4px;
}

.el-button {
  margin-right: 10px;
}
</style>