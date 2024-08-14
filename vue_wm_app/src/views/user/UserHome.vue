<script setup>  
import { ref, onMounted } from 'vue';  
import { useRouter } from 'vue-router';
import { useStore } from "vuex";

const store = useStore();    
const router = useRouter();
const user = ref({}); // 初始化用户信息对象  

onMounted(() => {  
  console.log(store); // 确认 store 存在  
  const userData = store.state.user;
  console.log(userData);  
  if (userData) {  
    user.value = userData;  
  } else {  
    // 用户未登录，跳转到登录页面  
    router.push('/login');
  }  
}); 
</script>  

<template>  
    <div>  
        <h1>欢迎，{{user.userId}}</h1>  
        <!-- 其他用户信息 --> 
    </div>  
</template>  