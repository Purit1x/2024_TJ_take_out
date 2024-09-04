<template>
    <div v-if="!isUserInfo" class="box">
      <div class="head">用户列表</div>
      <div class="top">
        <input type="text" v-model="searchQuery" placeholder="搜索用户名或电话号码" @keyup.enter="handleSearch" class="inputtext" />
        <button @click="handleSearch" class="search">搜索</button>
      </div>
  
      <el-table :data="showUserInfo" :border="parentBorder" style="width:100%" class="table">
        <el-table-column type="expand">
          <template #default="props">
            <div style="margin-left: 10%;">
              <p>用户名：{{ props.row.userName }}</p>
              <p>电话号码：{{ props.row.phoneNumber }}</p>
              <p>钱包余额：{{ props.row.wallet | currencyFilter }}</p>
              <!-- 其他用户信息 -->
              <el-button @click="deleteUser(props.row.userId)" type="danger">删除</el-button>
            </div>
          </template>
        </el-table-column>
        <el-table-column label="用户名" prop="userName" />
        <el-table-column label="电话号码" prop="phoneNumber" />
      </el-table>
      <el-button @click="gobackHome" class="return">返回</el-button>
    </div>
  </template>
  
  <script setup>
  import { ref, onMounted } from 'vue';
  import { ElMessage, ElMessageBox } from "element-plus";
  import { useRouter } from 'vue-router';
  import { getAllUsers, deleteUser } from "@/api/user"; // 确保正确导入 getAllUsers 和 deleteUser
  
  const router = useRouter();
  const userInfo = ref([]); // 存储所有用户信息
  const showUserInfo = ref([]); // 显示用户信息列表
  const searchQuery = ref(''); // 搜索框内容
  const isUserInfo = ref(false); // 是否显示用户信息
  const parentBorder = ref(false); // 是否显示边框
  
  onMounted(async () => {
    await fetchUsers();
  });
  
  const fetchUsers = async () => {
    try {
      const { data } = await getAllUsers();
      userInfo.value = data;
      showUserInfo.value = data;
    } catch (error) {
      ElMessage.error('获取用户列表失败');
    }
  };
  
  const handleSearch = () => {
    const query = searchQuery.value.trim().toLowerCase();
    if (query) {
      showUserInfo.value = userInfo.value.filter(user =>
        user.userName.toLowerCase().includes(query) ||
        user.phoneNumber.includes(query)
      );
    } else {
      showUserInfo.value = userInfo.value;
    }
  };
  
  const deleteUser = async (userId) => {
    ElMessageBox.confirm('确认要删除该用户吗?', '提示', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    }).then(async () => {
      try {
        await deleteUser(userId);
        ElMessage.success('删除成功');
        fetchUsers(); // 重新获取用户列表
      } catch (error) {
        ElMessage.error('删除失败');
      }
    }).catch(() => {});
  };
  
  const gobackHome = () => {
    router.push('/platform-home');
  };
  
  // 货币过滤器
  const currencyFilter = (value) => {
    if (value) {
      return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
    }
    return '$0.00';
  };
  </script>