<script setup>
import { useStore } from "vuex";
import { ElMessage } from "element-plus";
import { ref, onMounted, watch } from 'vue';  
import { useRouter } from 'vue-router';
import {getEcoInfo} from '@/api/platform';

const store = useStore();
const router = useRouter();
const gobackHome = () => {
    router.push('/platform-home');
}

const ecoInfo = ref(0)

onMounted(() => {
    getEcoInfo().then(res => {  // 环保订单比例
        console.log(res)
        ecoInfo.value = res;  
       }).catch(err => {  
        ElMessage.error('获取环保比例失败'); 
    }); 
})

</script>

<template>
    <div>
        <h2>
            订单管理

            <button @click="gobackHome">返回</button>
        </h2>            
        <p> 环保订单比例： {{ecoInfo.ecoOrderRatio}}</p>
       
    </div>
</template>