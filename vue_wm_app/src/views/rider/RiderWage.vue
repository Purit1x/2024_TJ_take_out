<script setup>
import { ref, onMounted, onBeforeUnmount } from 'vue';
import { useStore } from 'vuex';
import { useRoute } from 'vue-router';
import { getOrdersWithinThisMonth } from '@/api/rider';

const rider = ref({});
onMounted(async () => {
    const riderData = store.state.rider;  // 登录后，对应骑手信息会被存储到本地，将本地存储的骑手信息存储至riderData中
    if (riderData) {
        rider.value = riderData;
        const res = await riderInfo(rider.value.RiderId);
        rider.value = res.data;
        console.log('骑手信息', rider.value);
    }
    else {
        router.push('/login');  // 未登录，跳转至登陆界面
    }
})
</script>

<template>
    <div>
        <br><br>
        <h2>您所在的位置：工资记录</h2>
    </div>
</template>

<style scoped></style>