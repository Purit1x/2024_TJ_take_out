<script setup>  
import { ref, onMounted } from 'vue';  
import { useCookies } from '@vueuse/integrations/useCookies'  
import { useRouter } from 'vue-router';
const router = useRouter()
const merchant = ref({}); // 初始化商家信息对象  
const cookies = useCookies(); // 使用 useCookies 钩子  

onMounted(() => {  
  // 从 cookie 中读取用户信息  
  const merchantData = cookies.get('merchant');  
  console.log(merchantData);  
  if (merchantData) {  
    merchant.value = merchantData;  
  } else {  
    // 用户未登录，跳转到登录页面  
    router.push('/login');
  }  
});  
</script>  

<template>  
    <div>  
        <h1>欢迎，{{merchant.MerchantId}}</h1>  
        <!-- 其他用户信息 --> 
    </div>  
</template>  