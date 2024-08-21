<script setup>
import { useStore } from "vuex";
import { ElMessage } from "element-plus";
import { ref, onMounted, watch } from 'vue';  
import { useRouter } from 'vue-router';
const store = useStore();
const router = useRouter();
const gobackHome = () => {
    router.push('/merchant-home');
}
</script>

<template>
    <div>
        满减活动
        <button @click="gobackHome">返回</button>
    </div>
</template>