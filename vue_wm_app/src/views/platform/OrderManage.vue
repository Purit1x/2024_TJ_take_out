<script setup>
import { useStore } from "vuex";
import { ElMessage } from "element-plus";
import { ref, onMounted, watch } from 'vue';  
import { useRouter } from 'vue-router';
const store = useStore();
const router = useRouter();
const gobackHome = () => {
    router.push('/platform-home');
}
</script>

<template>
    <div>
        <h2>
            订单管理
            <button @click="gobackHome">返回</button>
        </h2>
    </div>
</template>