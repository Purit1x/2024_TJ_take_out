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
      <el-table-column label="操作" width="120">
        <template #default="scope">
          <el-button @click="handleDeleteUser(scope.row.userId)" type="danger" size="small">删除</el-button>
        </template>
      </el-table-column>
    </el-table>
    <button @click="gobackHome" class="return">返回</button>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue';
import { ElMessage, ElMessageBox } from "element-plus";
import { getAllUsers, deleteUser } from "@/api/user"; // 确保正确导入 getAllUsers 和 deleteUser

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
    if (response && response.data) {
      users.value = response.data;
      showUsers.value = response.data;
    }
  } catch (error) {
    ElMessage.error('获取用户列表失败');
  }
};

// 搜索用户
const handleSearch = () => {
  const query = searchQuery.value.trim().toLowerCase();
  if (query) {
    showUsers.value = users.value.filter(user =>
      user.userName.toLowerCase().includes(query) ||
      user.phoneNumber.includes(query)
    );
  } else {
    showUsers.value = users.value;
  }
};

// 删除用户
const handleDeleteUser = async (userId) => {
  ElMessageBox.confirm('确认要删除该用户吗?', '提示', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'warning'
  }).then(async () => {
    try {
      const response = await deleteUser(userId);
      ElMessage.success(response.msg);
      fetchUsers(); // 重新获取用户列表
    } catch (error) {
      ElMessage.error(error.response.data.msg);
    }
  }).catch(() => {});
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