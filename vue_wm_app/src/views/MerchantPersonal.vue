<script setup>  
import { ref, onMounted,watch} from 'vue';  
import { useRouter } from 'vue-router';
import { inject } from 'vue'; 
import store from '@/store';
const router = useRouter();
const merchant =inject('merchant');


 
onMounted(() => {  
    merchant.value=store.state.merchant;
});  
</script>

<template>  
    <div>  
        <h1>这里是个人主页页面，{{merchant.MerchantId}}</h1>  
        <!-- 其他用户信息 --> 
    </div>  
</template>  