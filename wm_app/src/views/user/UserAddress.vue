<script setup>
import { useRouter } from 'vue-router'
import {onMounted} from 'vue'
const router = useRouter()
onMounted(() => {  

})
const gobackHome = () => {
    router.push('/user-home');
}
</script>

<template>
    <div>地址页面&nbsp;&nbsp;
        <button @click="gobackHome">返回</button>
    </div>
</template>