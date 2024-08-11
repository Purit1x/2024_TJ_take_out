<script setup>  
import { ref, onMounted } from 'vue';  
import { useCookies } from '@vueuse/integrations/useCookies'  
import { useRouter } from 'vue-router';
const router = useRouter()
const user = ref({}); // 初始化用户信息对象  
const cookies = useCookies(); // 使用 useCookies 钩子  

onMounted(() => {  
  // 从 cookie 中读取用户信息  
  const userData = cookies.get('user');  
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