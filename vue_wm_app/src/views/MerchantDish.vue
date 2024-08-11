<script setup>  
import { ref, onMounted } from 'vue';  
import { useRouter } from 'vue-router';
import { inject } from 'vue'; 
const router = useRouter();
const merchant =inject('merchant');

 
onMounted(() => {  
      

});  
</script>

<template>  
    <div>  
        <h1>这里是菜单页面，{{ merchant.MerchantId }}</h1>  
        <!-- 其他用户信息 --> 
    </div>  
</template>  