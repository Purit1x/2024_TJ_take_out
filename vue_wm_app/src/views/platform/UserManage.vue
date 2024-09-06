

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
  <div class="box">
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
.box{
    padding: 20px;
    background-color: #7ac2ee;
    border: 2px solid #000000;
    border-radius: 20px;
    margin-right: 30px;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);

    font-size: 3vmin; /* 字体大小 */
    position: fixed; /* 固定定位 */
    top: 60px; /* 贴近顶部 */
    left: 50%; /* 水平居中 */
    transform: translateX(-50%); /* 修正水平居中 */
    width: 70%;
    height: 85%;
    overflow: auto;
    .el-table {
        max-height: 100%; 
        overflow-y: auto; 
    }
}
/* 添加滚动条样式 */
.box::-webkit-scrollbar {
    width: 8px; /* 滚动条宽度 */
}

.box::-webkit-scrollbar-track {
    background-color: #f0f0f0; /* 滚动条轨道颜色 */
}
.user-manage {
  padding: 20px;
  background-color: #f0f0f0;
  border-radius: 8px;
}

.head{
    display:flex;
    justify-content: center; /* 水平居中 */
    align-items: center; /* 垂直居中 */
    left:50%;
    font-size: 4vmin; /* 字体大小 */
    color:#000000;
}

.top {
    display: flex;
    justify-content: center;
    /* 水平居中 */
    align-items: center;
    /* 垂直居中 */
}

.search {
    padding: 5px 8px;
    /* 按钮内边距 */
    margin-right: 8px;
    /* 按钮右边距 */
    background-color: #FFC0CB;
    font-size: 2.2vmin;
    /* 字体大小 */
}

.inputtext {
  width: 300px;
  height: 40px;
  padding: 0 10px;
  border: 1px solid #ccc;
  border-radius: 4px;
}

.return {
    position: absolute;
    right: 10px;
    bottom: 10px;
    background-color: #FFC0CB;
}

.search:hover {
  background-color: #337ecc;
}

.table {
  width: 90%;
  height:70%;
  border: 1px solid #ccc;
  border-radius: 4px;
  margin-top: 2%;
  overflow-y: auto; /* 使订单区域可以滚动 */
  overflow: auto;
}
.table::-webkit-scrollbar {
    width: 8px; /* 滚动条宽度 */
}

.table::-webkit-scrollbar-track {
    background-color: #f0f0f0; /* 滚动条轨道颜色 */
}
.el-button {
  margin-right: 10px;
}
</style>