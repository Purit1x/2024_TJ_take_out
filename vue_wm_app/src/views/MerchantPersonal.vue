<script setup>  
import { ref, onMounted } from 'vue';  
import { useRouter } from 'vue-router';
import { inject } from 'vue'; 
const router = useRouter();
const merchant =inject('merchant');

 
onMounted(() => {  
    // 清空当前页面的内容  
    const parentContainer = document.querySelector('.parent-container'); // 假设您的父容器包含 class 'parent-container'  
    if (parentContainer) {  
        parentContainer.innerHTML = ''; // 清空内容  
    }  
});  
</script>

<template>  
    <div>  
        <h1>这里是个人主页页面，{{merchant.MerchantId}}</h1>  
        <!-- 其他用户信息 --> 
    </div>  
</template>  